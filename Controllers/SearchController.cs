using Microsoft.AspNetCore.Mvc;
using MusicPlayerAPI.Dtos.Response;
using MusicPlayerAPI.Helpers;
using MusicPlayerAPI.Services.Interfaces;

namespace MusicPlayerAPI.Controllers;

[Route("api/search")]
[ApiController]
public class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;

    public SearchController(ISearchService searchService)
    {
        _searchService = searchService;
    }

    [HttpGet]
    public async Task<ActionResult<SearchRespDto>> Search([FromQuery] string query)
    {
        int userId = UserHelper.GetUserId(User);

        var result = await _searchService.SearchAsync(query, userId);
        return Ok(result);
    }
}
