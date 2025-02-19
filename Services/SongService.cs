using Microsoft.EntityFrameworkCore;
using MusicPlayerAPI.Data;
using MusicPlayerAPI.Helpers;
using MusicPlayerAPI.Models;
using MusicPlayerAPI.Models.Dtos;
using MusicPlayerAPI.Models.Entities;

namespace MusicPlayerAPI.Services
{
    public class SongService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly string _uploadAudioFolderPath;
        private readonly string _uploadImageFolderPath;
        private static readonly string[] AllowedAudioExtensions = { ".mp3" };
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png" };

        public SongService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            _uploadAudioFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Media", "Songs");
            _uploadImageFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Media", "Images");
            Directory.CreateDirectory(_uploadAudioFolderPath);
            Directory.CreateDirectory(_uploadImageFolderPath);
        }

        public async Task<StatusResult<List<Song>>> GetAll()
        {
            var songs = await _dbContext.Songs.ToListAsync();
            if (songs == null || !songs.Any()) return StatusResult<List<Song>>.Failure(404, "No songs found.");

            return StatusResult<List<Song>>.Success(songs, 200);
        }

        public async Task<StatusResult<Song>> GetById(int id)
        {
            var song = await _dbContext.Songs.FindAsync(id);
            if (song == null) return StatusResult<Song>.Failure(404, "Song not found.");

            return StatusResult<Song>.Success(song, 200);
        }

        public StatusResult<FileStream> Stream(string songPath)
        {
            try
            {
                if (!File.Exists(songPath)) return StatusResult<FileStream>.Failure(404, "Song file not found.");

                var fileStream = File.OpenRead(songPath);
                return StatusResult<FileStream>.Success(fileStream, 200);
            }
            catch (Exception ex)
            {
                return StatusResult<FileStream>.Failure(500, $"Error occurred while opening the file: {ex.Message}");
            }
        }

        public async Task<StatusResult<Song>> Upload(SongDto songDto, IFormFile audioFile, IFormFile? coverImageFile)
        {
            if (songDto == null || audioFile == null || !FileHelper.IsValidFile(audioFile, AllowedAudioExtensions))
                return StatusResult<Song>.Failure(400, "Invalid song data or audio file.");

            var existingSong = await _dbContext.Songs.AnyAsync(p => p.Title == songDto.Title && p.Artist == songDto.Artist);
            if (existingSong) return StatusResult<Song>.Failure(400, "Song already exists.");

            var audioFileName = FileHelper.SaveFile(audioFile, _uploadAudioFolderPath);

            var coverImagePath = coverImageFile != null && FileHelper.IsValidFile(coverImageFile, AllowedImageExtensions)
                ? FileHelper.SaveFile(coverImageFile, _uploadImageFolderPath)
                : FileHelper.GetDefaultCoverImagePath(_uploadImageFolderPath);

            var song = new Song
            {
                Title = songDto.Title,
                Artist = songDto.Artist,
                AudioFilePath = audioFileName,
                CoverImagePath = coverImagePath
            };

            await _dbContext.Songs.AddAsync(song);
            await _dbContext.SaveChangesAsync();

            return StatusResult<Song>.Success(song, 200, "Song uploaded successfully.");
        }

        public async Task<StatusResult<Song>> UpdateCoverImage(int songId)
        {
            var song = await _dbContext.Songs.FindAsync(songId);
            if (song == null) return StatusResult<Song>.Failure(404, "Song not found.");

            FileHelper.DeleteFile(song.CoverImagePath);
            song.CoverImagePath = FileHelper.GetDefaultCoverImagePath(_uploadImageFolderPath);

            await _dbContext.SaveChangesAsync();

            return StatusResult<Song>.Success(song, 200, "Cover image successfully removed and set to default");
        }

        public async Task<StatusResult<Song>> Update(int id, SongDto songDto, IFormFile audioFile, IFormFile coverImageFile)
        {
            var resp = await GetById(id);
            if (resp.Status != 200) return StatusResult<Song>.Failure(resp.Status, resp.Message);

            var song = resp.Data;

            if (song == null) return StatusResult<Song>.Failure(404, "Song not found.");

            var existingSong = await _dbContext.Songs.AnyAsync(s => s.Id != id && s.Title == songDto.Title && s.Artist == songDto.Artist);
            if (existingSong) return StatusResult<Song>.Failure(409, "A song with the same title and artist already exists.");

            if (coverImageFile != null && FileHelper.IsValidFile(coverImageFile, AllowedImageExtensions))
            {
                FileHelper.DeleteFile(song.CoverImagePath);
                song.CoverImagePath = FileHelper.SaveFile(coverImageFile, _uploadImageFolderPath);
            }

            if (audioFile != null && FileHelper.IsValidFile(audioFile, AllowedAudioExtensions))
            {
                FileHelper.DeleteFile(song.AudioFilePath);
                song.AudioFilePath = FileHelper.SaveFile(audioFile, _uploadAudioFolderPath);
            }

            song.Title = songDto.Title;
            song.Artist = songDto.Artist;

            await _dbContext.SaveChangesAsync();

            return StatusResult<Song>.Success(song, 200, "Song updated successfully.");
        }

        public async Task<StatusResult<Song>> Delete(int id)
        {
            var resp = await GetById(id);

            if (resp.Status != 200) return StatusResult<Song>.Failure(resp.Status, resp.Message);
            var song = resp.Data;

            if (song == null) return StatusResult<Song>.Failure(404, "Song not found");

            try
            {
                FileHelper.DeleteFile(song.AudioFilePath);
                FileHelper.DeleteFile(song.CoverImagePath);
                _dbContext.Songs.Remove(song);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusResult<Song>.Failure(500, $"Error occurred while deleting: {ex.Message}");
            }

            return StatusResult<Song>.Success(song, 200, "Song deleted successfully");
        }
    }
}
