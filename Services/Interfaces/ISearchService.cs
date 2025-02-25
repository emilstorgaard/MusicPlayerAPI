using MusicPlayerAPI.Models.Entities;

namespace MusicPlayerAPI.Services.Interfaces;

public interface ISearchService
{
    Task<StatusResult<(List<Playlist>, List<Song>)>> SearchAsync(string query);
}