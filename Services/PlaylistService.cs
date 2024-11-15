using Microsoft.EntityFrameworkCore;
using MusicPlayerAPI.Data;
using MusicPlayerAPI.Models.Dtos;
using MusicPlayerAPI.Models.Entities;

namespace MusicPlayerAPI.Services
{
    public class PlaylistService : IPlaylistService
    {
        public readonly ApplicationDbContext _dbContext;

        public PlaylistService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Playlist>> GetAll()
        {
            return await _dbContext.Playlists.ToListAsync();
        }

        public async Task<Playlist> GetPlaylistByIdAsync(int id)
        {
            // Fetch the playlist from the database using the playlistId
            var playlist = await _dbContext.Playlists
                //.Include(p => p.Songs) // Include related songs if needed
                .FirstOrDefaultAsync(p => p.Id == id);

            return playlist;
        }

        public async Task<bool?> AddPlaylist(PlaylistDto playlistDto)
        {
            if (playlistDto == null) return false;

            var playlist = new Playlist
            {
                Name = playlistDto.Name
            };

            await _dbContext.Playlists.AddAsync(playlist);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool?> UpdatePlaylist(int id, PlaylistDto playlistDto)
        {
            var playlist = await _dbContext.Playlists.FirstOrDefaultAsync(t => t.Id == id);
            if (playlist == null) return null;

            playlist.Name = playlistDto.Name;

            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool?> DeletePlaylist(int id)
        {
            var playlist = await _dbContext.Playlists.FirstOrDefaultAsync(t => t.Id == id);
            if (playlist == null) return null;

            _dbContext.Playlists.Remove(playlist);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> AddSongToPlaylistAsync(int playlistId, int songId)
        {
            var playlist = await _dbContext.Playlists.FirstOrDefaultAsync(p => p.Id == playlistId);
            if (playlist == null) return false;

            var song = await _dbContext.Songs.FirstOrDefaultAsync(s => s.Id == songId);
            if (song == null) return false;

            var exists = await _dbContext.PlaylistSongs.AnyAsync(ps => ps.PlaylistId == playlistId && ps.SongId == songId);
            if (exists) return true; // Song is already in the playlist

            var playlistSong = new PlaylistSong
            {
                PlaylistId = playlistId,
                SongId = songId
            };

            _dbContext.PlaylistSongs.Add(playlistSong);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<Song>> GetAllSongsByPlaylistIdAsync(int playlistId)
        {
            var playlistSongs = await _dbContext.PlaylistSongs
                .Where(ps => ps.PlaylistId == playlistId)
                .Include(ps => ps.Song) // Include related Song data
                .Select(ps => ps.Song)  // Select only the Song object
                .ToListAsync();

            return playlistSongs;
        }
    }
}
