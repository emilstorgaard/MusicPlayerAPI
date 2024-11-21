using Microsoft.EntityFrameworkCore;
using MusicPlayerAPI.Data;
using MusicPlayerAPI.Models.Dtos;
using MusicPlayerAPI.Models.Entities;

namespace MusicPlayerAPI.Services
{
    public class PlaylistService
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

        public async Task<Playlist?> GetById(int id)
        {
           var playlist = await _dbContext.Playlists.FirstOrDefaultAsync(p => p.Id == id);

            return playlist;
        }

        public async Task<bool> Add(PlaylistDto playlistDto)
        {
            if (playlistDto == null) return false;

            var existingPlaylist = await _dbContext.Playlists
                .FirstOrDefaultAsync(p => p.Name == playlistDto.Name);

            if (existingPlaylist != null) return false;

            var copenhagenTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
            var playlist = new Playlist
            {
                Name = playlistDto.Name,
                CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, copenhagenTimeZone)
            };

            await _dbContext.Playlists.AddAsync(playlist);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> Update(int id, PlaylistDto playlistDto)
        {
            if (playlistDto == null) return false;

            var existingPlaylist = await _dbContext.Playlists
                .FirstOrDefaultAsync(p => p.Name == playlistDto.Name);

            if (existingPlaylist != null) return false;

            var playlist = await _dbContext.Playlists.FirstOrDefaultAsync(t => t.Id == id);
            if (playlist == null) return false;

            playlist.Name = playlistDto.Name;

            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> Delete(int id)
        {
            var playlist = await _dbContext.Playlists.FirstOrDefaultAsync(t => t.Id == id);
            if (playlist == null) return false;

            _dbContext.Playlists.Remove(playlist);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> AddToPlaylist(int playlistId, int songId)
        {
            var exists = await _dbContext.PlaylistSongs.AnyAsync(ps => ps.PlaylistId == playlistId && ps.SongId == songId);
            if (exists) return false;

            var playlistSong = new PlaylistSong
            {
                PlaylistId = playlistId,
                SongId = songId
            };

            _dbContext.PlaylistSongs.Add(playlistSong);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<Song>> GetAllByPlaylistId(int playlistId)
        {
            var playlistSongs = await _dbContext.PlaylistSongs
                .Where(ps => ps.PlaylistId == playlistId)
                .Include(ps => ps.Song)
                .Select(ps => ps.Song)
                .ToListAsync();

            return playlistSongs;
        }

        public async Task<bool> RemoveFromPlaylist(int playlistId, int songId)
        {
            var playlistSong = await _dbContext.PlaylistSongs
                .FirstOrDefaultAsync(ps => ps.PlaylistId == playlistId && ps.SongId == songId);

            if (playlistSong == null) return false;

            _dbContext.PlaylistSongs.Remove(playlistSong);
            await _dbContext.SaveChangesAsync();

            return true;
        }
    }
}
