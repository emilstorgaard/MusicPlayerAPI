using Microsoft.EntityFrameworkCore;
using MusicPlayerAPI.Data;
using MusicPlayerAPI.Models.Entities;

namespace MusicPlayerAPI.Services
{
    public class SearchService
    {
        public readonly ApplicationDbContext _dbContext;

        public SearchService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<(IEnumerable<Playlist>, IEnumerable<Song>)> SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return (Enumerable.Empty<Playlist>(), Enumerable.Empty<Song>());
            }

            query = query.ToLower();

            var playlists = await _dbContext.Playlists
                .Where(p => p.Name.ToLower().Contains(query))
                .ToListAsync();

            var songs = await _dbContext.Songs
                .Where(s => s.Title.ToLower().Contains(query) || s.Artist.ToLower().Contains(query))
                .ToListAsync();

            return (playlists, songs);
        }
    }
}
