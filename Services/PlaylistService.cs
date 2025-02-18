using Microsoft.EntityFrameworkCore;
using MusicPlayerAPI.Data;
using MusicPlayerAPI.Models.Dtos;
using MusicPlayerAPI.Models.Entities;

namespace MusicPlayerAPI.Services
{
    public class PlaylistService
    {
        private readonly string _uploadFolderPath;
        public readonly ApplicationDbContext _dbContext;

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
           return await _dbContext.Playlists.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<string?> GetCoverImagePathById(int id)
        {
            var playlist = await _dbContext.Playlists.FirstOrDefaultAsync(p => p.Id == id);
            if (playlist == null) return null;

            return playlist.CoverImagePath;
        }

        public async Task<bool> Add(PlaylistDto playlistDto, IFormFile coverImageFile)
        {
            if (playlistDto == null) return false;

            var existingPlaylist = await _dbContext.Playlists.FirstOrDefaultAsync(p => p.Name == playlistDto.Name);
            if (existingPlaylist != null) return false;

            var filePath = Path.Combine(_uploadFolderPath, "default.jpg");

            if (coverImageFile != null)
            {
                var allowedImageExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var imageFileExtension = Path.GetExtension(coverImageFile.FileName).ToLower();

                if (!allowedImageExtensions.Contains(imageFileExtension)) return false;

                var coverImageFileName = Guid.NewGuid().ToString() + imageFileExtension;

                filePath = Path.Combine(_uploadFolderPath, coverImageFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await coverImageFile.CopyToAsync(stream);
                }

            }

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
            if (playlistDto == null) return false;

            var existingPlaylist = await _dbContext.Playlists.FirstOrDefaultAsync(p => p.Name == playlistDto.Name && p.Id != id);
            if (existingPlaylist != null) return false;

            var playlist = await _dbContext.Playlists.FirstOrDefaultAsync(t => t.Id == id);
            if (playlist == null) return false;

            if (coverImageFile != null)
            {
                var allowedImageExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var imageFileExtension = Path.GetExtension(coverImageFile.FileName).ToLower();

                if (!allowedImageExtensions.Contains(imageFileExtension)) return false;

                var coverImageFileName = Guid.NewGuid().ToString() + imageFileExtension;
                var filePath = Path.Combine(_uploadFolderPath, coverImageFileName);

                if (!string.IsNullOrEmpty(playlist.CoverImagePath) &&
                    !playlist.CoverImagePath.EndsWith("default.jpg") &&
                    File.Exists(playlist.CoverImagePath))
                {
                    File.Delete(playlist.CoverImagePath);
                }

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await coverImageFile.CopyToAsync(stream);
                }

                playlist.CoverImagePath = filePath;
            }

            playlist.Name = playlistDto.Name;

            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> Delete(int id)
        {
            var playlist = await _dbContext.Playlists.FirstOrDefaultAsync(t => t.Id == id);
            if (playlist == null) return false;

            if (!string.IsNullOrEmpty(playlist.CoverImagePath) &&
           !playlist.CoverImagePath.EndsWith("default.jpg") &&
           File.Exists(playlist.CoverImagePath))
            {
                File.Delete(playlist.CoverImagePath);
            }

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
            var playlistSong = await _dbContext.PlaylistSongs.FirstOrDefaultAsync(ps => ps.PlaylistId == playlistId && ps.SongId == songId);
            if (playlistSong == null) return false;

            _dbContext.PlaylistSongs.Remove(playlistSong);
            await _dbContext.SaveChangesAsync();

            return true;
        }
    }
}
