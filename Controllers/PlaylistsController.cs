using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicPlayerAPI.Dtos.Request;
using MusicPlayerAPI.Dtos.Response;
using MusicPlayerAPI.Helpers;
using MusicPlayerAPI.Services;
using MusicPlayerAPI.Services.Interfaces;

namespace MusicPlayerAPI.Controllers;

[Route("api/playlists")]
[ApiController]
public class PlaylistsController : ControllerBase
{
    private readonly IPlaylistService _playlistService;

    public PlaylistsController(IPlaylistService playlistService)
    {
        _playlistService = playlistService;
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<List<PlaylistRespDto>>> GetAllByUserId()
    {
        int userId = UserHelper.GetUserId(User);

        var result = await _playlistService.GetAllByUserId(userId);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PlaylistRespDto>> GetById(int id)
    {
        int userId = UserHelper.GetUserId(User);

        var result = await _playlistService.GetById(id, userId);
        return Ok(result);
    }

    [HttpGet("cover/{*imagePath}")]
    public IActionResult GetCoverImageById(string imagePath)
    {
        var coverImagePath = _playlistService.GetCoverImagePath(imagePath);
        return PhysicalFile(coverImagePath, "image/jpeg");
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Add([FromForm] PlaylistReqDto playlistDto)
    {
        int userId = UserHelper.GetUserId(User);

        await _playlistService.Add(playlistDto, userId);
        return StatusCode(201, "Playlist was successfully added");
    }

    [Authorize]
    [HttpPost("{id:int}/like")]
    public async Task<IActionResult> Like(int id)
    {
        int userId = UserHelper.GetUserId(User);

        await _playlistService.Like(id, userId);
        return Ok("Playlist was liked successfully");
    }

    [Authorize]
    [HttpPost("{id:int}/dislike")]
    public async Task<IActionResult> Dislike(int id)
    {
        int userId = UserHelper.GetUserId(User);

        await _playlistService.Dislike(id, userId);
        return Ok("Playlist was successfully disliked");
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromForm] PlaylistReqDto playlistDto)
    {
        int userId = UserHelper.GetUserId(User);

        await _playlistService.Update(id, playlistDto, userId);
        return Ok("Playlist was successfully updated");
    }

    [Authorize]
    [HttpPut("{id:int}/cover/remove")]
    public async Task<IActionResult> RemoveCoverImageById(int id)
    {
        int userId = UserHelper.GetUserId(User);

        await _playlistService.UpdateCoverImage(id, userId);
        return Ok("Playlist cover image was successfully removed");
    }

    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        int userId = UserHelper.GetUserId(User);

        await _playlistService.Delete(id, userId);
        return Ok("Playlist was successfully deleted");
    }

    [Authorize]
    [HttpPost("{playlistId}/songs/{songId}")]
    public async Task<IActionResult> AddSongToPlaylist(int playlistId, int songId)
    {
        int userId = UserHelper.GetUserId(User);
        
        await _playlistService.AddToPlaylist(playlistId, songId, userId);
        return Ok("Song was successfully added to playlist");
    }

    [HttpGet("{id}/songs")]
    public async Task<ActionResult<List<SongRespDto>>> GetAllSongsByPlaylistId(int id)
    {
        int userId = UserHelper.GetUserId(User);

        var result = await _playlistService.GetAllSongsByPlaylistId(id, userId);
        return Ok(result);
    }

    [Authorize]
    [HttpDelete("{playlistId}/songs/{songId}")]
    public async Task<IActionResult> RemoveSongFromPlaylist(int playlistId, int songId)
    {
        int userId = UserHelper.GetUserId(User);

        await _playlistService.RemoveFromPlaylist(playlistId, songId, userId);
        return Ok("Song was successfully removed from playlist");
    }
}
