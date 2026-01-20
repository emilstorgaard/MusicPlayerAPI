using MusicPlayerAPI.Dtos.Response;
using MusicPlayerAPI.Exceptions;
using MusicPlayerAPI.Mappers;
using MusicPlayerAPI.Repositories.Interfaces;
using MusicPlayerAPI.Services.Interfaces;

namespace MusicPlayerAPI.Services;

public class SearchService : ISearchService
{
    private readonly ISearchRepository _searchRepository;
    private readonly IPlaylistRepository _playlistRepository;
    private readonly ISongRepository _songRepository;

    public SearchService(ISearchRepository searchRepository, IPlaylistRepository playlistRepository, ISongRepository songRepository)
    {  
        _searchRepository = searchRepository;
        _playlistRepository = playlistRepository;
        _songRepository = songRepository;
    }

    private bool IsValidQuery(string q, out string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            errorMessage = "Search query cannot be empty.";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }

    public async Task<SearchRespDto> SearchAsync(string q, int userId)
    {
        if (!IsValidQuery(q, out var errorMessage)) throw new BadRequestException($"Invalid search: {errorMessage}");

        q = q.ToLower();

        var playlists = await _searchRepository.GetPlaylistsBySearch(q);

        var likedPlaylistIds = await _playlistRepository.GetLikedPlaylistIdsByUser(userId);
        var playlistsDto =  playlists.Select(playlist =>
            PlaylistMapper.MapToDto(playlist, likedPlaylistIds.Contains(playlist.Id))
        ).ToList();

        var songs = await _searchRepository.GetSongsBySearch(q);

        var likedSongIds = await _songRepository.GetLikedSongIdsByUser(userId);
        var songsDto = songs.Select(song =>
            SongMapper.MapToDto(song, likedSongIds.Contains(song.Id))
        ).ToList();

        return new SearchRespDto
        {
            Playlists = playlistsDto,
            Songs = songsDto
        };
    }
}
