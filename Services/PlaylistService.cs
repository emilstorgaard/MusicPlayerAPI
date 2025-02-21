using Microsoft.EntityFrameworkCore;
using MusicPlayerAPI.Data;
using MusicPlayerAPI.Helpers;
using MusicPlayerAPI.Mappers;
using MusicPlayerAPI.Models.Dtos;
using MusicPlayerAPI.Models.Entities;

namespace MusicPlayerAPI.Services;

public class PlaylistService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly string _uploadFolderPath;
    private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png" };

    public PlaylistService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
        _uploadFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Media", "Images");
        Directory.CreateDirectory(_uploadFolderPath);
    }

    public async Task<StatusResult<List<PlaylistResDto>>> GetAll()
    {
        var playlists = await _dbContext.Playlists
            .Select(p => new
            {
                Playlist = p,
                IsLiked = _dbContext.LikedPlaylists.Any(lp => lp.PlaylistId == p.Id)
            })
            .ToListAsync();

        if (playlists == null || !playlists.Any())
            return StatusResult<List<PlaylistResDto>>.Failure(404, "No playlists found.");

        var playlistDtos = playlists.Select(p => PlaylistMapper.MapToDto(p.Playlist, p.IsLiked)).ToList();
        return StatusResult<List<PlaylistResDto>>.Success(playlistDtos, 200);
    }


    public async Task<StatusResult<PlaylistResDto>> GetById(int id)
    {
        var playlist = await _dbContext.Playlists.FindAsync(id);
        if (playlist == null) return StatusResult<PlaylistResDto>.Failure(404, "Playlist not found.");

        bool isLiked = await _dbContext.LikedPlaylists.AnyAsync(lp => lp.PlaylistId == id);
        var playlistDto = PlaylistMapper.MapToDto(playlist, isLiked);
        return StatusResult<PlaylistResDto>.Success(playlistDto, 200);
    }

    public async Task<StatusResult> Add(PlaylistReqDto playlistDto, IFormFile? coverImageFile, int userId)
    {
        if (playlistDto == null)
            return StatusResult.Failure(400, "Playlist data is required.");

        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null) return StatusResult.Failure(404, "User not found.");

        var existingPlaylist = await _dbContext.Playlists.AnyAsync(p => p.Name == playlistDto.Name && p.UserId == userId);
        if (existingPlaylist)
            return StatusResult.Failure(400, "Playlist already exists.");

        var filePath = coverImageFile != null && FileHelper.IsValidFile(coverImageFile, AllowedImageExtensions)
            ? FileHelper.SaveFile(coverImageFile, _uploadFolderPath)
            : FileHelper.GetDefaultCoverImagePath(_uploadFolderPath);

        var copenhagenTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
        var playlist = new Playlist
        {
            UserId = userId,
            Name = playlistDto.Name,
            CoverImagePath = filePath,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
            User = user
        };

        await _dbContext.Playlists.AddAsync(playlist);
        await _dbContext.SaveChangesAsync();
        return StatusResult.Success(201);
    }

    public async Task<StatusResult> Like(int playlistId, int userId)
    {
        var playlist = await _dbContext.Playlists
            .Include(p => p.LikedPlaylists)
            .FirstOrDefaultAsync(p => p.Id == playlistId);

        if (playlist == null) return StatusResult.Failure(404, "Playlist not found.");

        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null) return StatusResult.Failure(404, "User not found.");

        bool isAlreadyLiked = playlist.LikedPlaylists.Any(lp => lp.PlaylistId == playlistId && lp.UserId == userId);
        if (isAlreadyLiked) return StatusResult.Failure(400, "Playlist already liked.");

        var likedPlaylist = new LikedPlaylist
        {
            UserId = userId,
            PlaylistId = playlistId,
            Playlist = playlist,
            User = user,
        };

        playlist.LikedPlaylists.Add(likedPlaylist);
        playlist.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        return StatusResult.Success(200);
    }

    public async Task<StatusResult> Dislike(int playlistId, int userId)
    {
        var playlist = await _dbContext.Playlists.FindAsync(playlistId);
        if (playlist == null) return StatusResult.Failure(404, "Song not found.");

        var likedPlaylist = await _dbContext.LikedPlaylists.FirstOrDefaultAsync(lp => lp.PlaylistId == playlistId && lp.UserId == userId);
        if (likedPlaylist == null) return StatusResult.Failure(404, "Playlist not found in your liked playlists.");

        playlist.UpdatedAtUtc = DateTime.UtcNow;

        _dbContext.LikedPlaylists.Remove(likedPlaylist);
        await _dbContext.SaveChangesAsync();
        return StatusResult.Success(200);
    }

    public async Task<StatusResult> UpdateCoverImage(int playlistId, int userId)
    {
        var playlist = await _dbContext.Playlists.FirstOrDefaultAsync(p => p.Id == playlistId && p.UserId == userId);
        if (playlist == null) return StatusResult.Failure(404, "Playlist not found for user.");

        FileHelper.DeleteFile(playlist.CoverImagePath);
        playlist.CoverImagePath = FileHelper.GetDefaultCoverImagePath(_uploadFolderPath);
        playlist.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        return StatusResult.Success(200);
    }

    public async Task<StatusResult> Update(int id, PlaylistReqDto playlistDto, IFormFile? coverImageFile, int userId)
    {
        var playlist = await _dbContext.Playlists.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
        if (playlist == null) return StatusResult.Failure(404, "Your playlist was not found.");

        var existingPlaylist = await _dbContext.Playlists.AnyAsync(p => p.Id != id && p.Name == playlistDto.Name && p.UserId == userId);
        if (existingPlaylist) return StatusResult.Failure(409, "You already have a playlist with the same name.");

        if (coverImageFile != null && FileHelper.IsValidFile(coverImageFile, AllowedImageExtensions))
        {
            FileHelper.DeleteFile(playlist.CoverImagePath);
            playlist.CoverImagePath = FileHelper.SaveFile(coverImageFile, _uploadFolderPath);
        }

        playlist.Name = playlistDto.Name;
        playlist.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        return StatusResult.Success(200);
    }

    public async Task<StatusResult> Delete(int id, int userId)
    {
        var playlist = await _dbContext.Playlists.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
        if (playlist == null) return StatusResult.Failure(404, "Playlist not found for user.");

        try
        {
            FileHelper.DeleteFile(playlist.CoverImagePath);
            _dbContext.Playlists.Remove(playlist);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return StatusResult.Failure(500, $"Error occurred while deleting: {ex.Message}");
        }

        return StatusResult.Success(200);
    }

    public async Task<StatusResult> AddToPlaylist(int playlistId, int songId, int userId)
    {
        var songOnPlaylist = await _dbContext.PlaylistSongs.AnyAsync(ps => ps.PlaylistId == playlistId && ps.SongId == songId);
        if (songOnPlaylist) return StatusResult.Failure(409, "Song already exists on the playlist.");

        var playlist = await _dbContext.Playlists.FindAsync(playlistId);
        var song = await _dbContext.Songs.FindAsync(songId);

        if (playlist == null || song == null || playlist.UserId != userId) return StatusResult.Failure(404, "Playlist or Song not found.");

        var playlistSong = new PlaylistSong
        {
            PlaylistId = playlistId,
            SongId = songId,
            Song = song,
            Playlist = playlist
        };

        playlist.UpdatedAtUtc = DateTime.UtcNow;

        _dbContext.PlaylistSongs.Add(playlistSong);
        await _dbContext.SaveChangesAsync();
        return StatusResult.Success(200);
    }

    public async Task<StatusResult<List<SongResDto>>> GetAllSongsByPlaylistId(int playlistId)
    {
        var songs = await _dbContext.PlaylistSongs
            .Where(ps => ps.PlaylistId == playlistId)
            .Select(ps => new
            {
                Song = ps.Song,
                IsLiked = _dbContext.LikedSongs.Any(ls => ls.SongId == ps.Song.Id)
            })
            .ToListAsync();

        var songDtos = songs.Select(s => SongMapper.MapToDto(s.Song, s.IsLiked)).ToList();

        if (!songDtos.Any())
            return StatusResult<List<SongResDto>>.Failure(404, "No songs found in the playlist.");

        return StatusResult<List<SongResDto>>.Success(songDtos, 200);
    }

    public async Task<StatusResult> RemoveFromPlaylist(int playlistId, int songId, int userId)
    {
        var playlistSong = await _dbContext.PlaylistSongs.FirstOrDefaultAsync(ps => ps.PlaylistId == playlistId && ps.SongId == songId);
        if (playlistSong == null) return StatusResult.Failure(404, "Song not found on the playlist.");

        _dbContext.PlaylistSongs.Remove(playlistSong);

        var playlist = await _dbContext.Playlists
            .FirstOrDefaultAsync(p => p.Id == playlistId);

        // Check if playlist is owned by the user
        if (playlist == null || playlist.UserId != userId)
            return StatusResult.Failure(404, "Playlist not found for user.");

        playlist.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        return StatusResult.Success(200);
    }
}
