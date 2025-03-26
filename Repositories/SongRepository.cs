using Microsoft.EntityFrameworkCore;
using MusicPlayerAPI.Database;
using MusicPlayerAPI.Models;
using MusicPlayerAPI.Repositories.Interfaces;

namespace MusicPlayerAPI.Repositories;

public class SongRepository : ISongRepository
{
    private readonly ApplicationDbContext _dbContext;

    public SongRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Song?> GetSongById(int id)
    {
        return await _dbContext.Songs.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<bool> SongExists(string title, string artist)
    {
        return await _dbContext.Songs.AnyAsync(s => s.Title == title && s.Artist == artist);
    }

    public async Task AddSong(Song song)
    {
        await _dbContext.Songs.AddAsync(song);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<LikedSong?> GetLikedSongByUser(int songId, int userId)
    {
        return await _dbContext.LikedSongs.FirstOrDefaultAsync(ls => ls.SongId == songId && ls.UserId == userId);
    }

    public async Task<List<int>> GetLikedSongIdsByUser(int userId)
    {
        return await _dbContext.LikedSongs
            .AsNoTracking()
            .Where(lp => lp.UserId == userId)
            .Include(lp => lp.Song)
            .Select(lp => lp.Song.Id)
            .ToListAsync();
    }

    public async Task<List<int>> GetLikedSongIdsFromPlaylist(int playlistId, int userId)
    {
        return await _dbContext.LikedSongs
            .Where(ls => ls.UserId == userId && _dbContext.PlaylistSongs
                .Where(ps => ps.PlaylistId == playlistId)
                .Select(ps => ps.SongId)
                .Contains(ls.SongId))
            .Select(ls => ls.SongId)
            .ToListAsync();
    }

    public async Task<bool> IsSongLikedByUser(int songId, int userId)
    {
        return await _dbContext.LikedSongs
            .AsNoTracking()
            .AnyAsync(ls => ls.SongId == songId && ls.UserId == userId);
    }

    public async Task LikeSong(LikedSong likedSong)
    {
        await _dbContext.LikedSongs.AddAsync(likedSong);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DislikeSong(LikedSong likedSong)
    {
        _dbContext.LikedSongs.Remove(likedSong);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateSong(Song song)
    {
        _dbContext.Songs.Update(song);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteSong(Song song)
    {
        _dbContext.Songs.Remove(song);
        await _dbContext.SaveChangesAsync();
    }
}
