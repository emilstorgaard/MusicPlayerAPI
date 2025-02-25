using Microsoft.EntityFrameworkCore;
using MusicPlayerAPI.Data;
using MusicPlayerAPI.Models.Entities;
using MusicPlayerAPI.Services.Interfaces;

namespace MusicPlayerAPI.Services;

public class SearchService : ISearchService
{
    private readonly ApplicationDbContext _dbContext;

    public SearchService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    private bool IsValidQuery(string query, out string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            errorMessage = "Search query cannot be empty.";
            return false;
        }

        if (query.Length < 3)
        {
            errorMessage = "Search query must be at least 3 characters long.";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }

    public async Task<StatusResult<(List<Playlist>, List<Song>)>> SearchAsync(string query)
    {
        if (!IsValidQuery(query, out var errorMessage)) return StatusResult<(List<Playlist>, List<Song>)>.Failure(400, errorMessage);

        query = query.ToLower();

        var playlists = await _dbContext.Playlists
            .AsNoTracking()
            .Where(p => p.Name.ToLower().Contains(query))
            .ToListAsync();

        var songs = await _dbContext.Songs
            .AsNoTracking()
            .Where(s => s.Title.ToLower().Contains(query) || s.Artist.ToLower().Contains(query))
            .ToListAsync();

        return StatusResult<(List<Playlist>, List<Song>)>.Success((playlists, songs), 200, "Search results found.");
    }
}
