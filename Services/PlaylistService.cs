using MusicPlayerAPI.Dtos.Request;
using MusicPlayerAPI.Dtos.Response;
using MusicPlayerAPI.Exceptions;
using MusicPlayerAPI.Helpers;
using MusicPlayerAPI.Mappers;
using MusicPlayerAPI.Entities;
using MusicPlayerAPI.Repositories.Interfaces;
using MusicPlayerAPI.Services.Interfaces;
using MusicPlayerAPI.Configurations;

namespace MusicPlayerAPI.Services;

public class PlaylistService : IPlaylistService
{
    private readonly Settings _settings;
    private readonly ISongService _songService;
    private readonly IPlaylistRepository _playlistRepository;
    private readonly IUserRepository _userRepository;
    private readonly ISongRepository _songRepository;

    public PlaylistService(Settings settings, ISongService songService, IPlaylistRepository playlistRepository, IUserRepository userRepository, ISongRepository songRepository)
    {
        _settings = settings;
        Directory.CreateDirectory(_settings.UploadImageFolderPath);
        _songService = songService;
        _playlistRepository = playlistRepository;
        _userRepository = userRepository;
        _songRepository = songRepository;
    }

    public async Task<List<PlaylistRespDto>> GetAllByUserId(int userId)
    {
        var playlists = await _playlistRepository.GetAllPlaylistsByUserId(userId);
        var likedPlaylistIds = await _playlistRepository.GetLikedPlaylistIdsByUser(userId);

        return playlists.Select(playlist =>
            PlaylistMapper.MapToDto(playlist, likedPlaylistIds.Contains(playlist.Id))
        ).ToList();
    }

    public async Task<PlaylistRespDto> GetById(int id, int userId)
    {
        var playlist = await _playlistRepository.GetPlaylistById(id);
        if (playlist == null) throw new NotFoundException("Playlist not found.");

        var isLiked = await _playlistRepository.IsPlaylistLikedByUser(id, userId);

        var playlistDto = PlaylistMapper.MapToDto(playlist, isLiked);
        return playlistDto;
    }

    public string GetCoverImagePath(string imagePath)
    {
        var coverImagePath = FileHelper.GetFullPath(imagePath);

        if (!System.IO.File.Exists(coverImagePath)) throw new NotFoundException($"Cover image not found on path: {coverImagePath}.");

        return coverImagePath;
    }

    public async Task Add(PlaylistReqDto playlistDto, int userId)
    {
        if (playlistDto == null) throw new BadHttpRequestException("Playlist data is missing.");

        var user = await _userRepository.GetUserById(userId);
        if (user == null) throw new UnauthorizedException("Invalid user ID.");

        var existingPlaylist = await _playlistRepository.GetExistingPlaylist(playlistDto.Name, userId);
        if (existingPlaylist != null) throw new ConflictException("You already have a playlist with the same name.");

        var filePath = playlistDto.CoverImageFile != null && FileHelper.IsValidFile(playlistDto.CoverImageFile, _settings.AllowedImageExtensions)
            ? FileHelper.SaveFile(playlistDto.CoverImageFile, _settings.UploadImageFolderPath)
            : FileHelper.GetDefaultCoverImagePath(_settings.UploadImageFolderPath);

        var playlist = new Playlist
        {
            UserId = userId,
            Name = playlistDto.Name,
            CoverImagePath = filePath,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
            User = user
        };

        await _playlistRepository.AddPlaylist(playlist);
    }

    public async Task Like(int playlistId, int userId)
    {
        var isAlreadyLiked = await _playlistRepository.IsPlaylistLikedByUser(playlistId, userId);
        if (isAlreadyLiked) throw new ConflictException("Playlist already liked by user.");

        var likedPlaylist = new LikedPlaylist
        {
            UserId = userId,
            PlaylistId = playlistId
        };

        var playlist = await _playlistRepository.GetPlaylistById(playlistId);
        if (playlist == null) throw new NotFoundException("Playlist not found.");

        playlist.UpdatedAtUtc = DateTime.UtcNow;

        await _playlistRepository.LikePlaylist(likedPlaylist);
    }

    public async Task Dislike(int playlistId, int userId)
    {
        var likedPlaylist = await _playlistRepository.GetLikedPlaylistByUser(playlistId, userId);
        if (likedPlaylist == null) throw new ConflictException("Playlist not liked by user.");

        var playlist = await _playlistRepository.GetPlaylistById(playlistId);
        if (playlist == null) throw new NotFoundException("Playlist not found.");

        playlist.UpdatedAtUtc = DateTime.UtcNow;

        await _playlistRepository.DislikePlaylist(likedPlaylist);
    }

    public async Task UpdateCoverImage(int playlistId, int userId)
    {
        var playlist = await _playlistRepository.GetPlaylistById(playlistId);
        if (playlist == null) throw new NotFoundException("Playlist not found.");
        if (playlist.UserId != userId) throw new UnauthorizedException("You are not allowed to update this playlist.");

        FileHelper.DeleteFile(playlist.CoverImagePath);

        playlist.CoverImagePath = FileHelper.GetDefaultCoverImagePath(_settings.UploadImageFolderPath);
        playlist.UpdatedAtUtc = DateTime.UtcNow;

        await _playlistRepository.UpdatePlaylist(playlist);
    }

    public async Task Update(int id, PlaylistReqDto playlistDto, int userId)
    {
        var playlist = await _playlistRepository.GetPlaylistById(id);
        if (playlist == null) throw new NotFoundException("Playlist was not found.");
        if (playlist.UserId != userId) throw new UnauthorizedException("You are not allowed to update this playlist.");

        var existingPlaylist = await _playlistRepository.GetExistingPlaylist(playlistDto.Name, userId);
        if (existingPlaylist != null && existingPlaylist.Id != id) throw new ConflictException("You already have a playlist with the same name.");

        if (playlistDto.CoverImageFile != null && FileHelper.IsValidFile(playlistDto.CoverImageFile, _settings.AllowedImageExtensions))
        {
            FileHelper.DeleteFile(playlist.CoverImagePath);
            playlist.CoverImagePath = FileHelper.SaveFile(playlistDto.CoverImageFile, _settings.UploadImageFolderPath);
        }

        playlist.Name = playlistDto.Name;
        playlist.UpdatedAtUtc = DateTime.UtcNow;

        await _playlistRepository.UpdatePlaylist(playlist);
    }

    public async Task Delete(int id, int userId)
    {
        var playlist = await _playlistRepository.GetPlaylistById(id);
        if (playlist == null) throw new NotFoundException("Playlist not found.");
        if (playlist.UserId != userId) throw new UnauthorizedException("You are not allowed to delete this playlist.");

        FileHelper.DeleteFile(playlist.CoverImagePath);

        await _playlistRepository.DeletePlaylistSongs(id);
        await _playlistRepository.DeletePlaylist(playlist);
    }

    public async Task AddToPlaylist(int playlistId, int songId, int userId)
    {
        var playlist = await _playlistRepository.GetPlaylistById(playlistId);
        if (playlist == null) throw new NotFoundException("Playlist not found.");

        var songOnPlaylist = await _playlistRepository.IsSongInPlaylist(playlistId, songId);
        if (songOnPlaylist) throw new ConflictException("Song already exists in the playlist.");

        var song = await _songRepository.GetSongById(songId);
        if (playlist == null || song == null) throw new NotFoundException("Playlist or Song not found.");
        if (playlist.UserId != userId) throw new UnauthorizedException("Playlist not found for user.");

        var playlistSong = new PlaylistSong
        {
            PlaylistId = playlistId,
            SongId = songId
        };

        playlist.UpdatedAtUtc = DateTime.UtcNow;

        await _playlistRepository.AddSongToPlaylist(playlistSong);
    }

    public async Task<List<SongRespDto>> GetAllSongsByPlaylistId(int playlistId, int userId)
    {
        var songs = await _playlistRepository.GetSongsByPlaylistId(playlistId);

        var likedSongIds = await _songRepository.GetLikedSongIdsFromPlaylist(playlistId, userId);

        var songDtos = songs.Select(song =>
            SongMapper.MapToDto(song, likedSongIds.Contains(song.Id))
        ).ToList();

        return songDtos;
    }

    public async Task RemoveFromPlaylist(int playlistId, int songId, int userId)
    {
        var playlist = await _playlistRepository.GetPlaylistById(playlistId);
        if (playlist == null) throw new NotFoundException("Playlist not found.");
        if (playlist.UserId != userId) throw new UnauthorizedException("You are not allowed to modify this playlist.");

        var playlistSong = await _playlistRepository.GetPlaylistSong(playlistId, songId);
        if (playlistSong == null) throw new NotFoundException("Song not found in the playlist.");

        playlist.UpdatedAtUtc = DateTime.UtcNow;

        await _playlistRepository.RemoveSongFromPlaylist(playlistSong);
    }
}