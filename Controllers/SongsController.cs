using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicPlayerAPI.Dtos.Request;
using MusicPlayerAPI.Helpers;
using MusicPlayerAPI.Services.Interfaces;

namespace MusicPlayerAPI.Controllers;

[Route("api/songs")]
[ApiController]
public class SongsController : ControllerBase
{
    private readonly ISongService _songService;

    public SongsController(ISongService songService)
    {
        _songService = songService;
    }

    [HttpGet("{id:int}/stream")]
    public async Task<IActionResult> StreamSong(int id)
    {
        var streamResult = await _songService.Stream(id);
        return File(streamResult, "audio/mpeg", enableRangeProcessing: true);
    }

    [HttpGet("{imageFile:string}/cover")]
    public IActionResult GetCoverImage(string imageFile)
    {
        var coverImagePath = _songService.GetCoverImage(imageFile);
        return PhysicalFile(coverImagePath, "image/jpeg");
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> UploadSong([FromForm] SongReqDto songDto)
    {
        int userId = UserHelper.GetUserId(User);

        await _songService.Upload(songDto, userId);
        return StatusCode(201, "Song was successfully uploaded.");
    }

    [Authorize]
    [HttpPost("{id:int}/like")]
    public async Task<IActionResult> LikeSong(int id)
    {
        int userId = UserHelper.GetUserId(User);

        await _songService.Like(id, userId);
        return Ok("Song was successfully liked.");
    }

    [Authorize]
    [HttpPost("{id:int}/dislike")]
    public async Task<IActionResult> DislikeSong(int id)
    {
        int userId = UserHelper.GetUserId(User);

        await _songService.Dislike(id, userId);
        return Ok("Song was successfully disliked.");
    }

    [Authorize]
    [HttpPut("{id:int}/cover/remove")]
    public async Task<IActionResult> RemoveCoverImage(int id)
    {
        int userId = UserHelper.GetUserId(User);

        await _songService.UpdateCoverImage(id, userId);
        return Ok("Song cover image was successfully removed.");
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateSong(int id, [FromForm] SongReqDto songDto)
    {
        int userId = UserHelper.GetUserId(User);

        await _songService.Update(id, songDto, userId);
        return Ok("Song was successfully updated.");
    }

    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteSong(int id)
    {
        int userId = UserHelper.GetUserId(User);

        await _songService.Delete(id, userId);
        return Ok("Song was successfully deleted.");
    }
}
