using Microsoft.EntityFrameworkCore;
using MusicPlayerAPI.Data;
using MusicPlayerAPI.Models.Dtos;
using MusicPlayerAPI.Models.Entities;

namespace MusicPlayerAPI.Services
{
    public class SongService : ISongService
    {
        private readonly string _uploadFolderPath;
        public readonly ApplicationDbContext _dbContext;

        public SongService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            _uploadFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Songs");
            Directory.CreateDirectory(_uploadFolderPath);
        }

        public FileStream Stream(string filename)
        {
            var filePath = Path.Combine(_uploadFolderPath, filename);
            if (File.Exists(filePath))
            {
                return new FileStream(filePath, FileMode.Open, FileAccess.Read);
            }
            else
            {
                return null;
            }
        }

        public async Task<List<Song>> GetAll()
        {
            return await _dbContext.Songs.ToListAsync();
        }

        public async Task<Song> GetSongByIdAsync(int id)
        {
            // Fetch the song from the database using the songId
            var song = await _dbContext.Songs
                .FirstOrDefaultAsync(s => s.Id == id);

            return song;
        }

        public async Task UploadSong(SongDto songDto, IFormFile file)
        {
            var filePath = Path.Combine(_uploadFolderPath, file.FileName);

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
        }

        public async Task<bool?> UpdateSong(int id, SongDto songDto)
        {
            var song = await _dbContext.Songs.FirstOrDefaultAsync(t => t.Id == id);
            if (song == null) return null;

            song.Title = songDto.Title;
            song.Artist = songDto.Artist;

            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task DeleteSong(int id)
        {
            var song = await _dbContext.Songs.FindAsync(id);

            if (song != null)
            {
                // Delete the file from the file system
                if (File.Exists(song.FilePath))
                {
                    File.Delete(song.FilePath);
                }

                // Remove the song from the database
                _dbContext.Songs.Remove(song);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
