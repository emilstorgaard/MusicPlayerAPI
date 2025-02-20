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

        public async Task<StatusResult<List<Playlist>>> GetAll()
        {
            var playlists = await _dbContext.Playlists.ToListAsync();
            if (playlists == null || !playlists.Any())
                return StatusResult<List<Playlist>>.Failure(404, "No playlists found.");

            return StatusResult<List<Playlist>>.Success(playlists, 200);
        }


        public async Task<StatusResult<Playlist>> GetById(int id)
        {
            var playlist = await _dbContext.Playlists.FindAsync(id);
            if (playlist == null) return StatusResult<Playlist>.Failure(404, "Playlist not found.");

            return StatusResult<Playlist>.Success(playlist, 200);
        }

        public async Task<StatusResult> Add(PlaylistDto playlistDto, IFormFile? coverImageFile)
        {
            if (playlistDto == null)
                return StatusResult.Failure(400, "Playlist data is required.");

            var existingPlaylist = await _dbContext.Playlists.AnyAsync(p => p.Name == playlistDto.Name);
            if (existingPlaylist)
                return StatusResult.Failure(400, "Playlist already exists.");

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

            return StatusResult.Success(201);
        }


        public async Task<StatusResult> UpdateCoverImage(int playlistId)
        {
            var playlist = await _dbContext.Playlists.FindAsync(playlistId);
            if (playlist == null) return StatusResult.Failure(404, "Playlist not found.");

            FileHelper.DeleteFile(playlist.CoverImagePath);
            playlist.CoverImagePath = FileHelper.GetDefaultCoverImagePath(_uploadFolderPath);

            await _dbContext.SaveChangesAsync();

            return StatusResult.Success(200);
        }

        public async Task<StatusResult> Update(int id, PlaylistDto playlistDto, IFormFile? coverImageFile)
        {
            var result = await GetById(id);
            if (result.Status != 200) return StatusResult.Failure(result.Status, result.Message ?? "An error occurred.");

            var playlist = result.Data;

            if (playlist == null) return StatusResult.Failure(404, "Playlist not found.");

            var existingPlaylist = await _dbContext.Playlists.AnyAsync(p => p.Id != id && p.Name == playlistDto.Name);
            if (existingPlaylist) return StatusResult.Failure(409, "A playlist with the same name already exists.");

            if (coverImageFile != null && FileHelper.IsValidFile(coverImageFile, AllowedImageExtensions))
            {
                FileHelper.DeleteFile(playlist.CoverImagePath);
                playlist.CoverImagePath = FileHelper.SaveFile(coverImageFile, _uploadFolderPath);
            }

            playlist.Name = playlistDto.Name;

            await _dbContext.SaveChangesAsync();

            return StatusResult.Success(200);
        }

        public async Task<StatusResult> Delete(int id)
        {
            var result = await GetById(id);

            if (result.Status != 200) return StatusResult.Failure(result.Status, result.Message ?? "An error occurred.");
            var playlist = result.Data;

            if (playlist == null) return StatusResult.Failure(404, "Playlist not found.");

            try
            {
                FileHelper.DeleteFile(playlist.CoverImagePath);
                _dbContext.Playlists.Remove(playlist);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusResult.Failure(500, $"Error occurred while deleting: {ex.Message}");
            }

            return StatusResult.Success(200);
        }

        public async Task<StatusResult> AddToPlaylist(int playlistId, int songId)
        {
            var songOnPlaylist = await _dbContext.PlaylistSongs.AnyAsync(ps => ps.PlaylistId == playlistId && ps.SongId == songId);
            if (songOnPlaylist) return StatusResult.Failure(409, "Song already exists on the playlist.");

            var playlist = await _dbContext.Playlists.FindAsync(playlistId);
            var song = await _dbContext.Songs.FindAsync(songId);

            if (playlist == null || song == null) return StatusResult.Failure(404, "Playlist or Song not found.");

            var playlistSong = new PlaylistSong
            {
                PlaylistId = playlistId,
                SongId = songId,
                Song = song,
                Playlist = playlist
            };

            _dbContext.PlaylistSongs.Add(playlistSong);
            await _dbContext.SaveChangesAsync();

            return StatusResult.Success(200);
        }

        public async Task<StatusResult<List<Song>>> GetAllSongsByPlaylistId(int playlistId)
        {
            var playlistSongs = await _dbContext.PlaylistSongs
                .Where(ps => ps.PlaylistId == playlistId)
                .Include(ps => ps.Song)
                .Select(ps => ps.Song)
                .ToListAsync();

            return StatusResult<List<Song>>.Success(playlistSongs, 200);
        }

        public async Task<StatusResult> RemoveFromPlaylist(int playlistId, int songId)
        {
            var playlistSong = await _dbContext.PlaylistSongs.FirstOrDefaultAsync(ps => ps.PlaylistId == playlistId && ps.SongId == songId);
            if (playlistSong == null) return StatusResult.Failure(404, "Song not found on the playlist.");

            _dbContext.PlaylistSongs.Remove(playlistSong);
            await _dbContext.SaveChangesAsync();

            return StatusResult.Success(200);
        }
    }
}
