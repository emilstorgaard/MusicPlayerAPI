﻿using Microsoft.AspNetCore.Mvc;
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
            var playlists = await _playlistService.GetAll();
            return Ok(playlists);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var playlist = await _playlistService.GetById(id);
            if (playlist == null) return NotFound("Playlist not found");

            return Ok(playlist);
        }

        [HttpPost]
        public async Task<IActionResult> AddPlaylist(PlaylistDto playlistDto)
        {
            var result = await _playlistService.Add(playlistDto);
            if (!result) return BadRequest("Failed to add playlist");

            return Ok();
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdatePlaylist(int id, PlaylistDto playlistDto)
        {
            var result = await _playlistService.Update(id, playlistDto);
            if (!result) return BadRequest("Failed to update playlist");

            return Ok();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeletePlaylist(int id)
        {
            var result = await _playlistService.Delete(id);
            if (!result) return NotFound("Playlist not found");

            return Ok();
        }

        [HttpPost("{playlistId}/songs/{songId}")]
        public async Task<IActionResult> AddSongToPlaylist(int playlistId, int songId)
        {
            var playlist = await _playlistService.GetById(playlistId);
            if (playlist == null) return NotFound("Playlist not found");

            var song = await _songService.GetById(songId);
            if (song == null) return NotFound("Song not found");

            var result = await _playlistService.AddToPlaylist(playlistId, songId);
            if (!result) return BadRequest("Failed to add song to playlist");

            return Ok();
        }

        [HttpGet("{id}/songs")]
        public async Task<IActionResult> GetAllSongsByPlaylistId(int id)
        {
            var songs = await _playlistService.GetAllByPlaylistId(id);
            return Ok(songs);
        }

        [HttpDelete("{playlistId}/songs/{songId}")]
        public async Task<IActionResult> RemoveSongFromPlaylist(int playlistId, int songId)
        {
            var result = await _playlistService.RemoveFromPlaylist(playlistId, songId);
            if (!result) return NotFound("Song not found in the playlist.");

            return Ok();
        }
    }
}
