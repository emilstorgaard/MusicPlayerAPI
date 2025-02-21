using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicPlayerAPI.Models.Dtos;
using MusicPlayerAPI.Services;

namespace MusicPlayerAPI.Controllers;

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

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> AddPlaylist([FromForm] PlaylistReqDto playlistDto, [FromForm] IFormFile? coverImageFile)
    {
        var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Uid")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            return Unauthorized("Invalid or missing user ID in token.");

        var result = await _playlistService.Add(playlistDto, coverImageFile, userId);
        return StatusCode(result.Status, result.Message);
    }

    [Authorize]
    [HttpPost("{id:int}/like")]
    public async Task<IActionResult> LikePlaylist(int id)
    {
        var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Uid")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            return Unauthorized("Invalid or missing user ID in token.");

        var result = await _playlistService.Like(id, userId);
        return StatusCode(result.Status, result.Message);
    }

    [Authorize]
    [HttpPost("{id:int}/dislike")]
    public async Task<IActionResult> DislikePlaylist(int id)
    {
        var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Uid")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            return Unauthorized("Invalid or missing user ID in token.");

        var result = await _playlistService.Dislike(id, userId);
        return StatusCode(result.Status, result.Message);
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdatePlaylist(int id, [FromForm] PlaylistReqDto playlistDto, [FromForm] IFormFile? coverImageFile)
    {
        var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Uid")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            return Unauthorized("Invalid or missing user ID in token.");

        var result = await _playlistService.Update(id, playlistDto, coverImageFile, userId);
        return StatusCode(result.Status, result.Message);
    }

    [Authorize]
    [HttpPut("{id:int}/cover/remove")]
    public async Task<IActionResult> RemoveCoverImage(int id)
    {
        var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Uid")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            return Unauthorized("Invalid or missing user ID in token.");

        var result = await _playlistService.UpdateCoverImage(id, userId);
        return StatusCode(result.Status, result.Message);
    }

    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeletePlaylist(int id)
    {
        var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Uid")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            return Unauthorized("Invalid or missing user ID in token.");

        var result = await _playlistService.Delete(id, userId);
        return StatusCode(result.Status, result.Message);
    }

    [Authorize]
    [HttpPost("{playlistId}/songs/{songId}")]
    public async Task<IActionResult> AddSongToPlaylist(int playlistId, int songId)
    {
        var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Uid")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            return Unauthorized("Invalid or missing user ID in token.");

        var playlistResult = await _playlistService.GetById(playlistId);
        if (playlistResult.Status != 200) return StatusCode(playlistResult.Status, playlistResult.Message);

        var songResult = await _songService.GetById(songId);
        if (songResult.Status != 200) return StatusCode(songResult.Status, songResult.Message);

        var result = await _playlistService.AddToPlaylist(playlistId, songId, userId);
        return StatusCode(result.Status, result.Message);
    }

    [HttpGet("{id}/songs")]
    public async Task<IActionResult> GetAllSongsByPlaylistId(int id)
    {
        var result = await _playlistService.GetAllSongsByPlaylistId(id);
        if (result.Status != 200) return StatusCode(result.Status, result.Message);

        return Ok(result.Data);
    }

    [Authorize]
    [HttpDelete("{playlistId}/songs/{songId}")]
    public async Task<IActionResult> RemoveSongFromPlaylist(int playlistId, int songId)
    {
        var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Uid")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            return Unauthorized("Invalid or missing user ID in token.");

        var result = await _playlistService.RemoveFromPlaylist(playlistId, songId, userId);
        return StatusCode(result.Status, result.Message);
    }
}
