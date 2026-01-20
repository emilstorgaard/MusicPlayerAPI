using Microsoft.EntityFrameworkCore;
using MusicPlayerAPI.Data;
using MusicPlayerAPI.Entities;
using MusicPlayerAPI.Repositories.Interfaces;

namespace MusicPlayerAPI.Repositories;

public class SearchRepository : ISearchRepository
{
    private readonly ApplicationDbContext _dbContext;

    public SearchRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Playlist>> GetPlaylistsBySearch(string search)
    {
        return await _dbContext.Playlists
            .AsNoTracking()
            .Where(p => p.Name.ToLower().Contains(search))
            .ToListAsync();
    }

    public async Task<List<Song>> GetSongsBySearch(string search)
    {
        return await _dbContext.Songs
            .AsNoTracking()
            .Where(s => s.Title.ToLower().Contains(search) || s.Artist.ToLower().Contains(search))
            .ToListAsync();
    }
}
