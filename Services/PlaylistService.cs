using Microsoft.EntityFrameworkCore;
using MusicPlayerAPI.Data;
using MusicPlayerAPI.Helpers;
using MusicPlayerAPI.Models.Dtos;
using MusicPlayerAPI.Models.Entities;

namespace MusicPlayerAPI.Services
{
    public class PlaylistService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly string _uploadFolderPath;
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png" };

        public PlaylistService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            _uploadFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Media", "Images");
            Directory.CreateDirectory(_uploadFolderPath);
        }

        public async Task<List<Playlist>> GetAll()
        {
            return await _dbContext.Playlists.ToListAsync();
        }

        public async Task<Playlist?> GetById(int id)
        {
           return await _dbContext.Playlists.FindAsync(id);
        }

        public async Task<bool> Add(PlaylistDto playlistDto, IFormFile coverImageFile)
        {
            if (playlistDto == null) return false;

            var existingPlaylist = await _dbContext.Playlists.AnyAsync(p => p.Name == playlistDto.Name);
            if (existingPlaylist) return false;

            var filePath = coverImageFile != null && FileHelper.IsValidFile(coverImageFile, AllowedImageExtensions)
                ? FileHelper.SaveFile(coverImageFile, _uploadFolderPath)
                : FileHelper.GetDefaultCoverImagePath(_uploadFolderPath);

            var copenhagenTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
            var playlist = new Playlist
            {
                Name = playlistDto.Name,
                CoverImagePath = filePath,
                CreatedAtUtc = DateTime.UtcNow
            };

            await _dbContext.Playlists.AddAsync(playlist);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> Update(int id, PlaylistDto playlistDto, IFormFile coverImageFile)
        {
            var playlist = await GetById(id);
            if (playlist == null) return false;

            var existingPlaylist = await _dbContext.Playlists.AnyAsync(p => p.Id != id && p.Name == playlistDto.Name);
            if (existingPlaylist) return false;

            if (coverImageFile != null && FileHelper.IsValidFile(coverImageFile, AllowedImageExtensions))
            {
                FileHelper.DeleteFile(playlist.CoverImagePath);
                playlist.CoverImagePath = FileHelper.SaveFile(coverImageFile, _uploadFolderPath);
            }

            playlist.Name = playlistDto.Name;

            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> Delete(int id)
        {
            var playlist = await _dbContext.Playlists.FindAsync(id);
            if (playlist == null) return false;

            FileHelper.DeleteFile(playlist.CoverImagePath);

            _dbContext.Playlists.Remove(playlist);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> AddToPlaylist(int playlistId, int songId)
        {
            var songOnPlaylist = await _dbContext.PlaylistSongs.AnyAsync(ps => ps.PlaylistId == playlistId && ps.SongId == songId);
            if (songOnPlaylist) return false;

            var playlistSong = new PlaylistSong
            {
                PlaylistId = playlistId,
                SongId = songId
            };

            _dbContext.PlaylistSongs.Add(playlistSong);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<Song>> GetAllSongsByPlaylistId(int playlistId)
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
            var playlistSong = await _dbContext.PlaylistSongs.FirstOrDefaultAsync(ps => ps.PlaylistId == playlistId && ps.SongId == songId);
            if (playlistSong == null) return false;

            _dbContext.PlaylistSongs.Remove(playlistSong);
            await _dbContext.SaveChangesAsync();

            return true;
        }
    }
}
