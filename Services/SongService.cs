using Microsoft.EntityFrameworkCore;
using MusicPlayerAPI.Data;
using MusicPlayerAPI.Models.Dtos;
using MusicPlayerAPI.Models.Entities;

namespace MusicPlayerAPI.Services
{
    public class SongService
    {
        private readonly string _uploadFolderPath;
        public readonly ApplicationDbContext _dbContext;

        public SongService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            _uploadFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Songs");
            Directory.CreateDirectory(_uploadFolderPath);
        }

        public FileStream? Stream(string filePath)
        {
            if (!File.Exists(filePath)) return null;
            
            return new FileStream(filePath, FileMode.Open, FileAccess.Read);
        }

        public async Task<List<Song>> GetAll()
        {
            return await _dbContext.Songs.ToListAsync();
        }

        public async Task<Song?> GetById(int id)
        {
            return await _dbContext.Songs.FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<bool> Upload(SongDto songDto, IFormFile file)
        {
            if (songDto == null || file == null) return false;

            var existingSong = await _dbContext.Songs.FirstOrDefaultAsync(p => p.Title == songDto.Title && p.Artist == songDto.Artist);
            if (existingSong != null) return false;

            var songFolderName = $"{SanitizeForFileSystem(songDto.Title)} - {SanitizeForFileSystem(songDto.Artist)}";
            var songFolderPath = Path.Combine(_uploadFolderPath, songFolderName);

            Directory.CreateDirectory(songFolderPath);

            var filePath = Path.Combine(songFolderPath, file.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var song = new Song
            {
                Title = songDto.Title,
                Artist = songDto.Artist,
                FilePath = filePath,
            };

            await _dbContext.Songs.AddAsync(song);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> Update(int id, SongDto songDto)
        {
            if (songDto == null) return false;

            var song = await GetById(id);
            if (song == null) return false;

            var existingSong = await _dbContext.Songs.FirstOrDefaultAsync(p => p.Title == songDto.Title && p.Artist == songDto.Artist);
            if (existingSong != null) return false;

            var oldFolderPath = Path.Combine(_uploadFolderPath, $"{SanitizeForFileSystem(song.Title)} - {SanitizeForFileSystem(song.Artist)}");
            var newFolderPath = Path.Combine(_uploadFolderPath, $"{SanitizeForFileSystem(songDto.Title)} - {SanitizeForFileSystem(songDto.Artist)}");

            if (!string.Equals(oldFolderPath, newFolderPath, StringComparison.OrdinalIgnoreCase))
            {
                if (Directory.Exists(oldFolderPath))
                {
                    Directory.Move(oldFolderPath, newFolderPath);
                }
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
            
            if (File.Exists(song.FilePath)) File.Delete(song.FilePath);

            _dbContext.Songs.Remove(song);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        private static string SanitizeForFileSystem(string input)
        {
            foreach (var invalidChar in Path.GetInvalidFileNameChars())
            {
                input = input.Replace(invalidChar, '_');
            }
            return input;
        }
    }
}
