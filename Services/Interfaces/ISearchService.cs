using MusicPlayerAPI.Dtos.Response;

namespace MusicPlayerAPI.Services.Interfaces;

public interface ISearchService
{
    Task<SearchRespDto> SearchAsync(string query, int userId);
}