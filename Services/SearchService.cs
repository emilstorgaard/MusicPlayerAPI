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

    private bool IsValidQuery(string query, out string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            errorMessage = "Search query cannot be empty.";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }

    public async Task<SearchRespDto> SearchAsync(string query, int userId)
    {
        if (!IsValidQuery(query, out var errorMessage)) throw new BadRequestException($"Invalid search: {errorMessage}");

        query = query.ToLower();

        var playlists = await _searchRepository.GetPlaylistsBySearch(query);

        var likedPlaylistIds = await _playlistRepository.GetLikedPlaylistIdsByUser(userId);
        var playlistsDto =  playlists.Select(playlist =>
            PlaylistMapper.MapToDto(playlist, likedPlaylistIds.Contains(playlist.Id))
        ).ToList();

        var songs = await _searchRepository.GetSongsBySearch(query);

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
