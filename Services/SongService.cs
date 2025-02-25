﻿using Microsoft.EntityFrameworkCore;
using MusicPlayerAPI.Data;
using MusicPlayerAPI.Helpers;
using MusicPlayerAPI.Mappers;
using MusicPlayerAPI.Models.Dtos.Request;
using MusicPlayerAPI.Models.Dtos.Response;
using MusicPlayerAPI.Models.Entities;
using MusicPlayerAPI.Services.Interfaces;

namespace MusicPlayerAPI.Services;

public class SongService : ISongService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly string _uploadAudioFolderPath;
    private readonly string _uploadImageFolderPath;
    private readonly string[] _allowedAudioExtensions;
    private readonly string[] _allowedImageExtensions;

    public SongService(ApplicationDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _uploadAudioFolderPath = configuration["FilePaths:AudioFolder"] ?? throw new ArgumentNullException(nameof(configuration), "Audio folder path is not configured.");
        _uploadImageFolderPath = configuration["FilePaths:ImageFolder"] ?? throw new ArgumentNullException(nameof(configuration), "Image folder path is not configured.");
        _allowedAudioExtensions = configuration.GetSection("FileTypes:AudioExtensions").Get<string[]>() ?? throw new ArgumentNullException(nameof(configuration), "AudioExtensions is missing.");
        _allowedImageExtensions = configuration.GetSection("FileTypes:ImageExtensions").Get<string[]>() ?? throw new ArgumentNullException(nameof(configuration), "ImageExtensions is missing.");
        Directory.CreateDirectory(_uploadAudioFolderPath);
        Directory.CreateDirectory(_uploadImageFolderPath);
    }

    public async Task<StatusResult<List<SongRespDto>>> GetAll()
    {
        var songs = await _dbContext.Songs
            .AsNoTracking()
            .Select(s => new
            {
                Song = s,
                IsLiked = _dbContext.LikedSongs.Any(ls => ls.SongId == s.Id)
            })
            .ToListAsync();

        if (!songs.Any()) return StatusResult<List<SongRespDto>>.Failure(404, "No songs found.");

        var songDtos = songs.Select(s => SongMapper.MapToDto(s.Song, s.IsLiked)).ToList();
        return StatusResult<List<SongRespDto>>.Success(songDtos, 200);
    }

    public async Task<StatusResult<SongRespDto>> GetById(int id)
    {
        var song = await _dbContext.Songs.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        if (song == null) return StatusResult<SongRespDto>.Failure(404, "Song not found.");

        bool isLiked = await _dbContext.LikedSongs.AnyAsync(ls => ls.SongId == id);
        var songDto = SongMapper.MapToDto(song, isLiked);
        return StatusResult<SongRespDto>.Success(songDto, 200);
    }

    public StatusResult<FileStream> Stream(string songPath)
    {
        if (!File.Exists(songPath)) return StatusResult<FileStream>.Failure(404, "Song file not found.");

        try
        {
            var fileStream = File.OpenRead(songPath);
            return StatusResult<FileStream>.Success(fileStream, 200);
        }
        catch (Exception ex)
        {
            return StatusResult<FileStream>.Failure(500, $"Error occurred while opening the file: {ex.Message}");
        }
    }

    public async Task<StatusResult> Upload(SongReqDto songDto, IFormFile audioFile, IFormFile? coverImageFile, int userId)
    {
        if (songDto == null || audioFile == null || !FileHelper.IsValidFile(audioFile, _allowedAudioExtensions))
            return StatusResult.Failure(400, "Invalid song data or audio file.");

        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null) return StatusResult.Failure(404, "User not found.");

        var existingSong = await _dbContext.Songs.AnyAsync(p => p.Title == songDto.Title && p.Artist == songDto.Artist);
        if (existingSong) return StatusResult.Failure(400, "Song already exists.");

        var audioFilePath = FileHelper.SaveFile(audioFile, _uploadAudioFolderPath);
        var coverImagePath = coverImageFile != null && FileHelper.IsValidFile(coverImageFile, _allowedImageExtensions)
            ? FileHelper.SaveFile(coverImageFile, _uploadImageFolderPath)
            : FileHelper.GetDefaultCoverImagePath(_uploadImageFolderPath);

        var song = new Song
        {
            Title = songDto.Title,
            Artist = songDto.Artist,
            AudioFilePath = audioFilePath,
            CoverImagePath = coverImagePath,
            UserId = userId,
            CreatedAtUtc = TimeZoneHelper.GetCopenhagenTime(DateTime.UtcNow),
            UpdatedAtUtc = TimeZoneHelper.GetCopenhagenTime(DateTime.UtcNow),
            User = user
        };

        await _dbContext.Songs.AddAsync(song);
        await _dbContext.SaveChangesAsync();
        return StatusResult.Success(201);
    }

    public async Task<StatusResult> Like(int songId, int userId)
    {
        var song = await _dbContext.Songs.Include(s => s.LikedSongs).FirstOrDefaultAsync(s => s.Id == songId);
        if (song == null) return StatusResult.Failure(404, "Song not found.");

        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null) return StatusResult.Failure(404, "User not found.");

        bool isAlreadyLiked = song.LikedSongs.Any(ls => ls.SongId == songId && ls.UserId == userId);
        if (isAlreadyLiked) return StatusResult.Failure(400, "Song already liked.");

        var likedSong = new LikedSong
        {
            UserId = userId,
            SongId = songId,
            Song = song,
            User = user
        };
        song.LikedSongs.Add(likedSong);
        song.UpdatedAtUtc = TimeZoneHelper.GetCopenhagenTime(DateTime.UtcNow);

        await _dbContext.SaveChangesAsync();
        return StatusResult.Success(200);
    }

    public async Task<StatusResult> Dislike(int songId, int userId)
    {
        var song = await _dbContext.Songs.FindAsync(songId);
        if (song == null) return StatusResult.Failure(404, "Song not found.");

        var likedSong = await _dbContext.LikedSongs.FirstOrDefaultAsync(ls => ls.SongId == songId && ls.UserId == userId);
        if (likedSong == null) return StatusResult.Failure(404, "Song not found in your liked songs.");

        _dbContext.LikedSongs.Remove(likedSong);
        song.UpdatedAtUtc = TimeZoneHelper.GetCopenhagenTime(DateTime.UtcNow);

        await _dbContext.SaveChangesAsync();
        return StatusResult.Success(200);
    }

    public async Task<StatusResult> UpdateCoverImage(int songId, int userId)
    {
        var song = await _dbContext.Songs.FirstOrDefaultAsync(s => s.Id == songId && s.UserId == userId);
        if (song == null) return StatusResult.Failure(404, "Song not found for user.");

        FileHelper.DeleteFile(song.CoverImagePath);
        song.CoverImagePath = FileHelper.GetDefaultCoverImagePath(_uploadImageFolderPath);
        song.UpdatedAtUtc = TimeZoneHelper.GetCopenhagenTime(DateTime.UtcNow);

        await _dbContext.SaveChangesAsync();
        return StatusResult.Success(200);
    }

    public async Task<StatusResult> Update(int id, SongReqDto songDto, IFormFile? audioFile, IFormFile? coverImageFile, int userId)
    {
        var song = await _dbContext.Songs.FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
        if (song == null) return StatusResult.Failure(404, "Your song was not found.");

        var existingSong = await _dbContext.Songs.AnyAsync(s => s.Id != id && s.Title == songDto.Title && s.Artist == songDto.Artist);
        if (existingSong) return StatusResult.Failure(409, "A song with the same title and artist already exists.");

        if (coverImageFile != null && FileHelper.IsValidFile(coverImageFile, _allowedImageExtensions))
        {
            FileHelper.DeleteFile(song.CoverImagePath);
            song.CoverImagePath = FileHelper.SaveFile(coverImageFile, _uploadImageFolderPath);
        }

        if (audioFile != null && FileHelper.IsValidFile(audioFile, _allowedAudioExtensions))
        {
            FileHelper.DeleteFile(song.AudioFilePath);
            song.AudioFilePath = FileHelper.SaveFile(audioFile, _uploadAudioFolderPath);
        }

        song.Title = songDto.Title;
        song.Artist = songDto.Artist;
        song.UpdatedAtUtc = TimeZoneHelper.GetCopenhagenTime(DateTime.UtcNow);

        await _dbContext.SaveChangesAsync();
        return StatusResult.Success(200);
    }

    public async Task<StatusResult> Delete(int id, int userId)
    {
        var song = await _dbContext.Songs.FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
        if (song == null) return StatusResult<SongReqDto>.Failure(404, "Song not found for user.");

        try
        {
            FileHelper.DeleteFile(song.AudioFilePath);
            FileHelper.DeleteFile(song.CoverImagePath);
            _dbContext.Songs.Remove(song);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return StatusResult.Failure(500, $"Error occurred while deleting: {ex.Message}");
        }

        return StatusResult.Success(200);
    }
}
