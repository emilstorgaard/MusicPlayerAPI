using Microsoft.AspNetCore.Mvc;
using MusicPlayerAPI.Services;

namespace MusicPlayerAPI.Controllers
{
    [Route("api/search")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly SearchService _searchService;

        public SearchController(SearchService searchService)
        {
            _searchService = searchService;
        }

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return BadRequest("Search query cannot be empty.");

            if (query.Length < 3)
                return BadRequest("Search query must be at least 3 characters long.");

            var (playlists, songs) = await _searchService.SearchAsync(query);

            return Ok(new
            {
                Playlists = playlists,
                Songs = songs
            });
        }
    }
}
