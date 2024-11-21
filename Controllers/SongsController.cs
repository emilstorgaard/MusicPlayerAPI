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

        [HttpGet("{id:int}/stream")]
        public async Task<IActionResult> StreamSong(int id)
        {
            var song = await _songService.GetById(id);
            if (song == null) return NotFound();

            var fileStream = _songService.Stream(song.FilePath);
            if (fileStream == null) return NotFound();

            return File(fileStream, "audio/mpeg", enableRangeProcessing: true);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var songs = await _songService.GetAll();
            return Ok(songs);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var song = await _songService.GetById(id);
            if (song == null) return NotFound("Song not found");

            return Ok(song);
        }

        [HttpPost]
        public async Task<IActionResult> UploadSong([FromForm] SongDto songDto, [FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file uploaded.");

            var result = await _songService.Upload(songDto, file);
            if (!result) return NotFound("Failed to upload song");

            return Ok();
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateSong(int id, SongDto songDto)
        {
            var result = await _songService.Update(id, songDto);
            if (!result) return NotFound("Failed to update song");

            return Ok();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteSong(int id)
        {
            var result = await _songService.Delete(id);
            if (!result) return NotFound("Song not found");

            return Ok();
        }
    }
}
