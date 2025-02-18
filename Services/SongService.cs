using Microsoft.EntityFrameworkCore;
using MusicPlayerAPI.Data;
using MusicPlayerAPI.Models.Dtos;
using MusicPlayerAPI.Models.Entities;

namespace MusicPlayerAPI.Services
{
    public class SongService
    {
        private readonly string _uploadAudioFolderPath;
        private readonly string _uploadImageFolderPath;
        public readonly ApplicationDbContext _dbContext;

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
            if (!File.Exists(songPath)) return null;
            
            return new FileStream(songPath, FileMode.Open, FileAccess.Read);
        }

        public async Task<List<Song>> GetAll()
        {
            return await _dbContext.Songs.ToListAsync();
        }

        public async Task<Song?> GetById(int id)
        {
            return await _dbContext.Songs.FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<string?> GetCoverImagePathById(int id)
        {
            var song = await _dbContext.Songs.FirstOrDefaultAsync(p => p.Id == id);
            if (song == null) return null;

            return song.CoverImagePath;
        }

        public async Task<bool> Upload(SongDto songDto, IFormFile audioFile, IFormFile coverImageFile)
        {
            if (songDto == null || audioFile == null) return false;

            var allowedExtensions = new[] { ".mp3" };
            var audioFileExtension = Path.GetExtension(audioFile.FileName).ToLower();

            if (!allowedExtensions.Contains(audioFileExtension)) return false;

            var existingSong = await _dbContext.Songs.FirstOrDefaultAsync(p => p.Title == songDto.Title && p.Artist == songDto.Artist);
            if (existingSong != null) return false;

            var audioFileName = Guid.NewGuid().ToString() + audioFileExtension;

            var audioFilePath = Path.Combine(_uploadAudioFolderPath, audioFileName);
            using (var stream = new FileStream(audioFilePath, FileMode.Create))
            {
                await audioFile.CopyToAsync(stream);
            }

            var coverImagePath = Path.Combine(_uploadImageFolderPath, "default.jpg");

            if (coverImageFile != null)
            {
                var allowedImageExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var imageFileExtension = Path.GetExtension(coverImageFile.FileName).ToLower();

                if (!allowedImageExtensions.Contains(imageFileExtension)) return false;

                var coverImageFileName = Guid.NewGuid().ToString() + imageFileExtension;

                coverImagePath = Path.Combine(_uploadImageFolderPath, coverImageFileName);

                using (var stream = new FileStream(coverImagePath, FileMode.Create))
                {
                    await coverImageFile.CopyToAsync(stream);
                }
            }

            var song = new Song
            {
                Title = songDto.Title,
                Artist = songDto.Artist,
                AudioFilePath = audioFilePath,
                CoverImagePath = coverImagePath
            };

            await _dbContext.Songs.AddAsync(song);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> Update(int id, SongDto songDto, IFormFile audioFile, IFormFile coverImageFile)
        {
            if (songDto == null) return false;

            var song = await GetById(id);
            if (song == null) return false;

            var existingSong = await _dbContext.Songs.FirstOrDefaultAsync(p => p.Title == songDto.Title && p.Artist == songDto.Artist && p.Id != id);
            if (existingSong != null) return false;

            if (coverImageFile != null)
            {
                var allowedImageExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var imageFileExtension = Path.GetExtension(coverImageFile.FileName).ToLower();

                if (!allowedImageExtensions.Contains(imageFileExtension)) return false;

                var coverImageFileName = Guid.NewGuid().ToString() + imageFileExtension;
                var filePath = Path.Combine(_uploadImageFolderPath, coverImageFileName);

                if (!string.IsNullOrEmpty(song.CoverImagePath) &&
                    !song.CoverImagePath.EndsWith("default.jpg") &&
                    File.Exists(song.CoverImagePath))
                {
                    File.Delete(song.CoverImagePath);
                }

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await coverImageFile.CopyToAsync(stream);
                }

                song.CoverImagePath = filePath;
            }

            if (audioFile != null)
            {
                var allowedExtensions = new[] { ".mp3" };
                var audioFileExtension = Path.GetExtension(audioFile.FileName).ToLower();

                if (!allowedExtensions.Contains(audioFileExtension)) return false;

                var audioFileName = Guid.NewGuid().ToString() + audioFileExtension;

                var audioFilePath = Path.Combine(_uploadAudioFolderPath, audioFileName);

                File.Delete(song.AudioFilePath);

                using (var stream = new FileStream(audioFilePath, FileMode.Create))
                {
                    await audioFile.CopyToAsync(stream);
                }

                song.AudioFilePath = audioFilePath;
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
                File.Delete(song.AudioFilePath);
                File.Delete(song.CoverImagePath);
            }
            catch (Exception ex)
            {
                return false;
            }

            _dbContext.Songs.Remove(song);
            await _dbContext.SaveChangesAsync();

            return true;
        }
    }
}
