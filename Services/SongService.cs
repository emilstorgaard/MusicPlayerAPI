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

        public async Task<bool> Upload(SongDto songDto, IFormFile file)
        {
            if (songDto == null || file == null) return false;

            var allowedExtensions = new[] { ".mp3" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(fileExtension)) return false;

            var existingSong = await _dbContext.Songs.FirstOrDefaultAsync(p => p.Title == songDto.Title && p.Artist == songDto.Artist);
            if (existingSong != null) return false;

            var songFolderName = Guid.NewGuid().ToString();
            var songFolderPath = Path.Combine(_uploadFolderPath, songFolderName);

            Directory.CreateDirectory(songFolderPath);

            var filePath = Path.Combine(songFolderPath, "song.mp3");
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var song = new Song
            {
                Title = songDto.Title,
                Artist = songDto.Artist,
                FolderPath = songFolderPath,
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
                if (Directory.Exists(song.FolderPath))
                {
                    var songFiles = Directory.GetFiles(song.FolderPath);
                    foreach (var file in songFiles)
                    {
                        File.Delete(file);
                    }

                    Directory.Delete(song.FolderPath);
                }
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
