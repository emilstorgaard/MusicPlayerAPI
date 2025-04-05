using MusicPlayerAPI.Dtos.Request;
using MusicPlayerAPI.Exceptions;
using MusicPlayerAPI.Helpers;
using MusicPlayerAPI.Models;
using MusicPlayerAPI.Repositories.Interfaces;
using MusicPlayerAPI.Services.Interfaces;

namespace MusicPlayerAPI.Services;

public class SongService : ISongService
{
    private readonly Settings _settings;
    private readonly ISongRepository _songRepository;
    private readonly IUserRepository _userRepository;

    public SongService(Settings settings, ISongRepository songRepository, IUserRepository userRepository)
    {
        _settings = settings;
        Directory.CreateDirectory(_settings.UploadAudioFolderPath);
        Directory.CreateDirectory(_settings.UploadImageFolderPath);
        _songRepository = songRepository;
        _userRepository = userRepository;
    }

    public async Task<FileStream> Stream(int id)
    {
        var song = await _songRepository.GetSongById(id);
        if (song == null) throw new NotFoundException("Song not found.");

        if (string.IsNullOrEmpty(song?.AudioFilePath)) throw new NotFoundException("Song file path is missing.");

        var audioFilePath = FileHelper.GetFullPath(song.AudioFilePath);

        if (!File.Exists(audioFilePath)) throw new NotFoundException("Song file not found.");

        var fileStream = File.OpenRead(audioFilePath);
        return fileStream;
    }

    public string GetCoverImage(string imagePath)
    {
        var coverImagePath = FileHelper.GetFullPath(imagePath);

        if (!System.IO.File.Exists(coverImagePath)) throw new NotFoundException("Cover image not found.");

        return coverImagePath;
    }

    public async Task Upload(SongReqDto songDto, int userId)
    {
        if (songDto == null || songDto.AudioFile == null || !FileHelper.IsValidFile(songDto.AudioFile, _settings.AllowedAudioExtensions))
            throw new BadRequestException("Invalid song data.");

        var user = await _userRepository.GetUserById(userId);
        if (user == null) throw new NotFoundException("User not found.");

        var existingSong = await _songRepository.GetExsistingSong(songDto.Title, songDto.Artist);
        if (existingSong != null) throw new ConflictException("A song with the same title and artist already exists.");

        var audioFilePath = FileHelper.SaveFile(songDto.AudioFile, _settings.UploadAudioFolderPath);
        var coverImagePath = songDto.CoverImageFile != null && FileHelper.IsValidFile(songDto.CoverImageFile, _settings.AllowedImageExtensions)
            ? FileHelper.SaveFile(songDto.CoverImageFile, _settings.UploadImageFolderPath)
            : FileHelper.GetDefaultCoverImagePath(_settings.UploadImageFolderPath);

        var song = new Song
        {
            Title = songDto.Title,
            Artist = songDto.Artist,
            AudioFilePath = audioFilePath,
            CoverImagePath = coverImagePath,
            UserId = userId,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
            User = user
        };

        await _songRepository.AddSong(song);
    }

    public async Task Like(int songId, int userId)
    {   
        var isAlreadyLiked = await _songRepository.IsSongLikedByUser(songId, userId);
        if (isAlreadyLiked) throw new ConflictException("Song already liked.");

        var likedSong = new LikedSong
        {
            UserId = userId,
            SongId = songId
        };

        var song = await _songRepository.GetSongById(songId);
        if (song == null) throw new NotFoundException("Song not found.");

        song.UpdatedAtUtc = DateTime.UtcNow;

        await _songRepository.LikeSong(likedSong);
    }

    public async Task Dislike(int songId, int userId)
    {
        var likedSong = await _songRepository.GetLikedSongByUser(songId, userId);
        if (likedSong == null) throw new NotFoundException("Song not found in your liked songs.");

        var song = await _songRepository.GetSongById(songId);
        if (song == null) throw new NotFoundException("Song not found.");

        song.UpdatedAtUtc = DateTime.UtcNow;

        await _songRepository.DislikeSong(likedSong);
    }

    public async Task UpdateCoverImage(int songId, int userId)
    {
        var song = await _songRepository.GetSongById(songId);
        if (song == null) throw new NotFoundException("Song not found.");
        if (song.UserId != userId) throw new UnauthorizedException("You are not allowed to update this song.");

        FileHelper.DeleteFile(song.CoverImagePath);

        song.CoverImagePath = FileHelper.GetDefaultCoverImagePath(_settings.UploadImageFolderPath);
        song.UpdatedAtUtc = DateTime.UtcNow;

        await _songRepository.UpdateSong(song);
    }

    public async Task Update(int id, SongReqDto songDto, int userId)
    {
        var song = await _songRepository.GetSongById(id);
        if (song == null) throw new NotFoundException("Song was not found.");
        if (song.UserId != userId) throw new UnauthorizedException("You are not allowed to update this song.");

        var existingSong = await _songRepository.GetExsistingSong(songDto.Title, songDto.Artist);
        if (existingSong != null && existingSong.Id != id) throw new NotFoundException("A song with the same title and artist already exists.");

        if (songDto.CoverImageFile != null && FileHelper.IsValidFile(songDto.CoverImageFile, _settings.AllowedImageExtensions))
        {
            FileHelper.DeleteFile(song.CoverImagePath);
            song.CoverImagePath = FileHelper.SaveFile(songDto.CoverImageFile, _settings.UploadImageFolderPath);
        }

        if (songDto.AudioFile != null && FileHelper.IsValidFile(songDto.AudioFile, _settings.AllowedAudioExtensions))
        {
            FileHelper.DeleteFile(song.AudioFilePath);
            song.AudioFilePath = FileHelper.SaveFile(songDto.AudioFile, _settings.UploadAudioFolderPath);
        }

        song.Title = songDto.Title;
        song.Artist = songDto.Artist;
        song.UpdatedAtUtc = DateTime.UtcNow;

        await _songRepository.UpdateSong(song);
    }

    public async Task Delete(int id, int userId)
    {
        var song = await _songRepository.GetSongById(id);
        if (song == null) throw new NotFoundException("Song not found.");
        if (song.UserId != userId) throw new UnauthorizedException("You are not allowed to delete this song.");

        FileHelper.DeleteFile(song.AudioFilePath);
        FileHelper.DeleteFile(song.CoverImagePath);

        await _songRepository.DeleteSong(song);
    }
}
