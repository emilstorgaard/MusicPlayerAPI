using Microsoft.EntityFrameworkCore;
using MusicPlayerAPI.Data;
using MusicPlayerAPI.Entities;
using MusicPlayerAPI.Repositories.Interfaces;

namespace MusicPlayerAPI.Repositories;

public class PlaylistRepository : IPlaylistRepository
{
    private readonly ApplicationDbContext _dbContext;

    public PlaylistRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Playlist>> GetAllPlaylistsByUserId(int userId)
    {
        return await _dbContext.Playlists.AsNoTracking().Where(p => p.UserId == userId).ToListAsync();
    }

    public async Task<Playlist?> GetPlaylistById(int id)
    {
        return await _dbContext.Playlists.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Playlist?> GetExistingPlaylist(string name, int userId)
    {
        return await _dbContext.Playlists.AsNoTracking().FirstOrDefaultAsync(p => p.Name == name && p.User.Id == userId);
    }

    public async Task AddPlaylist(Playlist playlist)
    {
        await _dbContext.Playlists.AddAsync(playlist);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<LikedPlaylist?> GetLikedPlaylistByUser(int playlistId, int userId)
    {
        return await _dbContext.LikedPlaylists.FirstOrDefaultAsync(lp => lp.PlaylistId == playlistId && lp.UserId == userId);
    }

    public async Task<List<int>> GetLikedPlaylistIdsByUser(int userId)
    {
        return await _dbContext.LikedPlaylists
            .AsNoTracking()
            .Where(lp => lp.UserId == userId)
            .Include(lp => lp.Playlist)
            .Select(lp => lp.Playlist.Id)
            .ToListAsync();
    }

    public async Task<bool> IsPlaylistLikedByUser(int playlistId, int userId)
    {
        return await _dbContext.LikedPlaylists
            .AsNoTracking()
            .AnyAsync(lp => lp.PlaylistId == playlistId && lp.UserId == userId);
    }

    public async Task LikePlaylist(LikedPlaylist likedPlaylist)
    {
        await _dbContext.LikedPlaylists.AddAsync(likedPlaylist);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DislikePlaylist(LikedPlaylist likedPlaylist)
    {
        _dbContext.LikedPlaylists.Remove(likedPlaylist);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdatePlaylist(Playlist playlist)
    {
        _dbContext.Playlists.Update(playlist);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeletePlaylist(Playlist playlist)
    {
        _dbContext.Playlists.Remove(playlist);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeletePlaylistSongs(int playlistId)
    {
        var playlistSongs = _dbContext.PlaylistSongs.Where(ps => ps.PlaylistId == playlistId);
        _dbContext.PlaylistSongs.RemoveRange(playlistSongs);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteLikedPlaylists(int userId)
    {
        var likedPlaylists = _dbContext.LikedPlaylists.Where(lp => lp.UserId == userId);
        _dbContext.LikedPlaylists.RemoveRange(likedPlaylists);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> IsSongInPlaylist(int playlistId, int songId)
    {
        return await _dbContext.PlaylistSongs
            .AnyAsync(ps => ps.PlaylistId == playlistId && ps.SongId == songId);
    }

    public async Task AddSongToPlaylist(PlaylistSong playlistSong)
    {
        _dbContext.PlaylistSongs.Add(playlistSong);
        await _dbContext.SaveChangesAsync();        
    }

    public async Task<List<Song>> GetSongsByPlaylistId(int playlistId)
    {
        return await _dbContext.PlaylistSongs
            .AsNoTracking()
            .Where(ps => ps.PlaylistId == playlistId)
            .Include(ps => ps.Song)
            .Select(ps => ps.Song)
            .ToListAsync();
    }

    public async Task<PlaylistSong?> GetPlaylistSong(int playlistId, int songId)
    {
        return await _dbContext.PlaylistSongs
            .AsNoTracking()
            .FirstOrDefaultAsync(ps => ps.PlaylistId == playlistId && ps.SongId == songId);
    }

    public async Task RemoveSongFromPlaylist(PlaylistSong playlistSong)
    {
        _dbContext.PlaylistSongs.Remove(playlistSong);
        await _dbContext.SaveChangesAsync();
    }
}
