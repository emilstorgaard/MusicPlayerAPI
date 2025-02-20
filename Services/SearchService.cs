using Microsoft.EntityFrameworkCore;
using MusicPlayerAPI.Data;
using MusicPlayerAPI.Models.Entities;

namespace MusicPlayerAPI.Services
{
    public class SearchService
    {
        private readonly ApplicationDbContext _dbContext;

        public SearchService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<StatusResult<(List<Playlist>, List<Song>)>> SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return StatusResult<(List<Playlist>, List<Song>)>.Failure(400, "Search query cannot be empty.");

            if (query.Length < 3)
                return StatusResult<(List<Playlist>, List<Song>)>.Failure(400, "Search query must be at least 3 characters long.");

            query = query.ToLower();

            var playlists = await _dbContext.Playlists
                .Where(p => p.Name.ToLower().Contains(query))
                .ToListAsync();

            var songs = await _dbContext.Songs
                .Where(s => s.Title.ToLower().Contains(query) || s.Artist.ToLower().Contains(query))
                .ToListAsync();

            return StatusResult<(List<Playlist>, List<Song>)>.Success((playlists, songs), 200, "Search results found.");
        }
    }
}
