using Microsoft.AspNetCore.Mvc;
using MusicPlayerAPI.Models.Dtos;
using MusicPlayerAPI.Services;

namespace MusicPlayerAPI.Controllers
{
    [Route("api/songs")]
    [ApiController]
    public class SongsController : ControllerBase
    {
        private readonly SongService _songService;

        public SongsController(SongService songService)
        {
            _songService = songService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var response = await _songService.GetAll();

            if (response.Status != 200) return StatusCode(response.Status, response.Message);

            return Ok(response.Data);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _songService.GetById(id);

            if (response.Status != 200) return StatusCode(response.Status, response.Message);

            return Ok(response.Data);
        }

        [HttpGet("{id:int}/stream")]
        public async Task<IActionResult> StreamSong(int id)
        {
            var response = await _songService.GetById(id);
            if (response.Status != 200) return StatusCode(response.Status, response.Message);

            var song = response.Data;

            var streamResponse = _songService.Stream(song.AudioFilePath);
            if (streamResponse.Status != 200) return StatusCode(streamResponse.Status, streamResponse.Message);

            var fileStream = streamResponse.Data;
            if (fileStream == null) return NotFound("Audio file not found");

            return File(fileStream, "audio/mpeg", enableRangeProcessing: true);
        }

        [HttpGet("{id:int}/cover")]
        public async Task<IActionResult> GetCoverImage(int id)
        {
            var response = await _songService.GetById(id);
            if (response.Status != 200) return StatusCode(response.Status, response.Message);

            var song = response.Data;
            var coverImagePath = song?.CoverImagePath;

            if (coverImagePath == null || !System.IO.File.Exists(coverImagePath))
                return NotFound("Cover image not found.");

            return PhysicalFile(coverImagePath, "image/jpeg");
        }

        [HttpPost]
        public async Task<IActionResult> UploadSong([FromForm] SongDto songDto, [FromForm] IFormFile audioFile, [FromForm] IFormFile? coverImageFile)
        {
            if (audioFile == null || audioFile.Length == 0) return BadRequest("No audio file uploaded.");

            var result = await _songService.Upload(songDto, audioFile, coverImageFile);
            if (result.Status != 200) return StatusCode(result.Status, result.Message);

            return Ok(result.Data);
        }

        [HttpPut("{id:int}/cover/remove")]
        public async Task<IActionResult> RemoveCoverImage(int id)
        {
            var updateResult = await _songService.UpdateCoverImage(id);
            if (updateResult.Status != 200) return StatusCode(updateResult.Status, updateResult.Message);

            return Ok();
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateSong(int id, [FromForm] SongDto songDto, [FromForm] IFormFile? audioFile, [FromForm] IFormFile? coverImageFile)
        {
            var result = await _songService.Update(id, songDto, audioFile, coverImageFile);
            if (result.Status != 200) return StatusCode(result.Status, result.Message);

            return Ok(result.Data);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteSong(int id)
        {
            var result = await _songService.Delete(id);
            if (result.Status != 200) return StatusCode(result.Status, result.Message);

            return Ok(result.Data);
        }
    }
}
