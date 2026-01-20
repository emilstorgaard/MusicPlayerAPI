using MusicPlayerAPI.Dtos.Response;

namespace MusicPlayerAPI.Services.Interfaces;

public interface ISearchService
{
    Task<SearchRespDto> SearchAsync(string q, int userId);
}