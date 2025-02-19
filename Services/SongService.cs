using Microsoft.EntityFrameworkCore;
using MusicPlayerAPI.Data;
using MusicPlayerAPI.Helpers;
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

        public FileStream? Stream(string songPath)
        {
            return File.Exists(songPath) ? File.OpenRead(songPath) : null;
        }

        public async Task<List<Song>> GetAll()
        {
            return await _dbContext.Songs.ToListAsync();
        }

        public async Task<Song?> GetById(int id)
        {
            return await _dbContext.Songs.FindAsync(id);
        }

        public async Task<bool> Upload(SongDto songDto, IFormFile audioFile, IFormFile? coverImageFile)
        {
            if (songDto == null || audioFile == null || !FileHelper.IsValidFile(audioFile, AllowedAudioExtensions)) return false;

            var existingSong = await _dbContext.Songs.AnyAsync(p => p.Title == songDto.Title && p.Artist == songDto.Artist);
            if (existingSong) return false;

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

            return true;
        }

        public async Task<bool> Update(int id, SongDto songDto, IFormFile audioFile, IFormFile coverImageFile)
        {
            var song = await GetById(id);
            if (song == null) return false;

            var existingSong = await _dbContext.Songs.AnyAsync(s => s.Id != id && s.Title == songDto.Title && s.Artist == songDto.Artist);
            if (existingSong) return false;

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

            return true;
        }

        public async Task<bool> Delete(int id)
        {
            var song = await GetById(id);
            if (song == null) return false;
            
            try
            {
                FileHelper.DeleteFile(song.AudioFilePath);
                FileHelper.DeleteFile(song.CoverImagePath);
                _dbContext.Songs.Remove(song);
                await _dbContext.SaveChangesAsync();
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
