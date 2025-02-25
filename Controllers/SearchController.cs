using Microsoft.AspNetCore.Mvc;
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
    public async Task<IActionResult> Search([FromQuery] string query)
    {
        var result = await _searchService.SearchAsync(query);
        if (result.Status != 200) return StatusCode(result.Status, result.Message);

        var (playlists, songs) = result.Data;

        return Ok(new
        {
            Playlists = playlists,
            Songs = songs
        });
    }
}
