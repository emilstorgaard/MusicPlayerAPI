using Microsoft.AspNetCore.Mvc;
using MusicPlayerAPI.Models.Dtos;
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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var songs = await _songService.GetAll();
            if (songs == null || !songs.Any())
            {
                return NotFound("No songs available.");
            }

            return Ok(songs);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var song = await _songService.GetSongByIdAsync(id);
            if (song == null) return NotFound();

            return Ok(song);
        }

        [HttpPost]
        public async Task<IActionResult> UploadSong([FromForm] SongDto songDto, [FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            await _songService.UploadSong(songDto, file);

            return Ok(new { message = "Song uploaded successfully." });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateSong(int id, SongDto songDto)
        {
            var result = await _songService.UpdateSong(id, songDto);
            if (result == null) return NotFound();

            return Ok();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteSong(int id)
        {
            await _songService.DeleteSong(id);

            return Ok();
        }
    }
}
