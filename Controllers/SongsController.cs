using Microsoft.AspNetCore.Mvc;
using MusicPlayerAPI.Services;

namespace MusicPlayerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SongsController : ControllerBase
    {
        private readonly SongService _songService;

        public SongsController(SongService songService)
        {
            _songService = songService;
        }

        [HttpGet("stream/song/{filename}")]
        public IActionResult StreamSong(string filename)
        {
            var fileStream = _songService.Stream(filename);
            if (fileStream != null)
            {
                return File(fileStream, "audio/mpeg", enableRangeProcessing: true);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("shuffle")]
        public IActionResult ShuffleSongs()
        {
            var songs = _songService.GetAllSongs();
            if (songs == null || !songs.Any())
            {
                return NotFound("No songs available.");
            }

            var shuffledSongs = songs.OrderBy(s => Guid.NewGuid()).ToList();

            return Ok(shuffledSongs);
        }

        [HttpPost]
        public async Task<IActionResult> UploadSong([FromForm] IFormFile file)
        {
            var result = await _songService.UploadSong(file);

            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            return Ok(new { message = "Song uploaded successfully." });
        }
    }
}
