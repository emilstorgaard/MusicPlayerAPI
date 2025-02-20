using Microsoft.AspNetCore.Mvc;
using MusicPlayerAPI.Models.Dtos;
using MusicPlayerAPI.Services;

namespace MusicPlayerAPI.Controllers
{
    [Route("api/playlists")]
    [ApiController]
    public class PlaylistsController : ControllerBase
    {
        private readonly PlaylistService _playlistService;
        private readonly SongService _songService;

        public PlaylistsController(PlaylistService playlistService, SongService songService)
        {
            _playlistService = playlistService;
            _songService = songService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _playlistService.GetAll();
            if (result.Status != 200) return StatusCode(result.Status, result.Message);

            return Ok(result.Data);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _playlistService.GetById(id);
            if (result.Status != 200) return StatusCode(result.Status, result.Message);

            return Ok(result.Data);
        }

        [HttpGet("{id:int}/cover")]
        public async Task<IActionResult> GetCoverImage(int id)
        {
            var result = await _playlistService.GetById(id);
            if (result.Status != 200) return StatusCode(result.Status, result.Message);

            var playlist = result.Data;
            var coverImagePath = playlist?.CoverImagePath;
            if (coverImagePath == null || !System.IO.File.Exists(coverImagePath))
                return NotFound("Cover image not found.");

            return PhysicalFile(coverImagePath, "image/jpeg");
        }

        [HttpPost]
        public async Task<IActionResult> AddPlaylist([FromForm] PlaylistDto playlistDto, [FromForm] IFormFile? coverImageFile)
        {
            var result = await _playlistService.Add(playlistDto, coverImageFile);

            return StatusCode(result.Status, result.Message);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdatePlaylist(int id, [FromForm] PlaylistDto playlistDto, [FromForm] IFormFile? coverImageFile)
        {
            var result = await _playlistService.Update(id, playlistDto, coverImageFile);

            return StatusCode(result.Status, result.Message);
        }

        [HttpPut("{id:int}/cover/remove")]
        public async Task<IActionResult> RemoveCoverImage(int id)
        {
            var result = await _playlistService.UpdateCoverImage(id);

            return StatusCode(result.Status, result.Message);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeletePlaylist(int id)
        {
            var result = await _playlistService.Delete(id);

            return StatusCode(result.Status, result.Message);
        }

        [HttpPost("{playlistId}/songs/{songId}")]
        public async Task<IActionResult> AddSongToPlaylist(int playlistId, int songId)
        {
            var playlistResult = await _playlistService.GetById(playlistId);
            if (playlistResult.Status != 200) return StatusCode(playlistResult.Status, playlistResult.Message);

            var songResult = await _songService.GetById(songId);
            if (songResult.Status != 200) return StatusCode(songResult.Status, songResult.Message);

            var result = await _playlistService.AddToPlaylist(playlistId, songId);

            return StatusCode(result.Status, result.Message);
        }

        [HttpGet("{id}/songs")]
        public async Task<IActionResult> GetAllSongsByPlaylistId(int id)
        {
            var result = await _playlistService.GetAllSongsByPlaylistId(id);
            if (result.Status != 200) return StatusCode(result.Status, result.Message);

            return Ok(result.Data);
        }

        [HttpDelete("{playlistId}/songs/{songId}")]
        public async Task<IActionResult> RemoveSongFromPlaylist(int playlistId, int songId)
        {
            var result = await _playlistService.RemoveFromPlaylist(playlistId, songId);

            return StatusCode(result.Status, result.Message);
        }
    }
}
