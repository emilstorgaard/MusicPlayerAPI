﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicPlayerAPI.Models.Dtos;
using MusicPlayerAPI.Services;

namespace MusicPlayerAPI.Controllers;

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
        var result = await _songService.GetAll();
        if (result.Status != 200) return StatusCode(result.Status, result.Message);

        return Ok(result.Data);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _songService.GetById(id);
        if (result.Status != 200) return StatusCode(result.Status, result.Message);

        return Ok(result.Data);
    }

    [HttpGet("{id:int}/stream")]
    public async Task<IActionResult> StreamSong(int id)
    {
        var result = await _songService.GetById(id);
        if (result.Status != 200) return StatusCode(result.Status, result.Message);

        if (result.Data == null) return NotFound("Song not found.");

        var song = result.Data;
        var streamResult = _songService.Stream(song.AudioFilePath);
        if (streamResult.Status != 200) return StatusCode(streamResult.Status, streamResult.Message);

        var fileStream = streamResult.Data;
        if (fileStream == null) return NotFound("Audio file not found");

        return File(fileStream, "audio/mpeg", enableRangeProcessing: true);
    }

    [HttpGet("{id:int}/cover")]
    public async Task<IActionResult> GetCoverImage(int id)
    {
        var result = await _songService.GetById(id);
        if (result.Status != 200) return StatusCode(result.Status, result.Message);

        var song = result.Data;
        var coverImagePath = song?.CoverImagePath;

        if (coverImagePath == null || !System.IO.File.Exists(coverImagePath))
            return NotFound("Cover image not found.");

        return PhysicalFile(coverImagePath, "image/jpeg");
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> UploadSong([FromForm] SongReqDto songDto, [FromForm] IFormFile audioFile, [FromForm] IFormFile? coverImageFile)
    {
        var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Uid")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            return Unauthorized("Invalid or missing user ID in token.");

        var result = await _songService.Upload(songDto, audioFile, coverImageFile, userId);
        return StatusCode(result.Status, result.Message);
    }

    [Authorize]
    [HttpPost("{id:int}/like")]
    public async Task<IActionResult> LikeSong(int id)
    {
        var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Uid")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            return Unauthorized("Invalid or missing user ID in token.");

        var result = await _songService.Like(id, userId);
        return StatusCode(result.Status, result.Message);
    }

    [Authorize]
    [HttpPost("{id:int}/dislike")]
    public async Task<IActionResult> DislikeSong(int id)
    {
        var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Uid")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            return Unauthorized("Invalid or missing user ID in token.");

        var result = await _songService.Dislike(id, userId);
        return StatusCode(result.Status, result.Message);
    }

    [Authorize]
    [HttpPut("{id:int}/cover/remove")]
    public async Task<IActionResult> RemoveCoverImage(int id)
    {
        var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Uid")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            return Unauthorized("Invalid or missing user ID in token.");

        var result = await _songService.UpdateCoverImage(id, userId);
        return StatusCode(result.Status, result.Message);
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateSong(int id, [FromForm] SongReqDto songDto, [FromForm] IFormFile? audioFile, [FromForm] IFormFile? coverImageFile)
    {
        var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Uid")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            return Unauthorized("Invalid or missing user ID in token.");

        var result = await _songService.Update(id, songDto, audioFile, coverImageFile, userId);
        return StatusCode(result.Status, result.Message);
    }

    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteSong(int id)
    {
        var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Uid")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            return Unauthorized("Invalid or missing user ID in token.");

        var result = await _songService.Delete(id, userId);
        return StatusCode(result.Status, result.Message);
    }
}
