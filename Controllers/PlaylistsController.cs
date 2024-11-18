using Microsoft.AspNetCore.Mvc;
using MusicPlayerAPI.Models.Dtos;
using MusicPlayerAPI.Services;

namespace MusicPlayerAPI.Controllers
{
    [Route("api/[controller]")]
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
            if (playlists == null || !playlists.Any())
            {
                return NotFound("No playlists available.");
            }

            return Ok(playlists);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var playlist = await _playlistService.GetPlaylistByIdAsync(id);
            if (playlist == null) return NotFound();

            return Ok(playlist);
        }

        [HttpPost]
        public async Task<IActionResult> AddPlaylist(PlaylistDto playlistDto)
        {
            var result = await _playlistService.AddPlaylist(playlistDto);
            if ((bool)!result) return NotFound();

            return Ok();
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdatePlaylist(int id, PlaylistDto playlistDto)
        {
            var result = await _playlistService.UpdatePlaylist(id, playlistDto);
            if (result == null) return NotFound();

            return Ok();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeletePlaylist(int id)
        {
            var result = await _playlistService.DeletePlaylist(id);
            if ((bool)!result) return NotFound();

            return Ok();
        }

        [HttpPost("{playlistId}/songs/{songId}")]
        public async Task<IActionResult> AddSongToPlaylist(int playlistId, int songId)
        {
            var playlist = await _playlistService.GetPlaylistByIdAsync(playlistId);
            if (playlist == null) return NotFound("Playlist not found");

            var song = await _songService.GetSongByIdAsync(songId);
            if (song == null) return NotFound("Song not found");

            var result = await _playlistService.AddSongToPlaylistAsync(playlistId, songId);
            if (result) return Ok("Song added to playlist");

            return BadRequest("Failed to add song to playlist");
        }

        [HttpGet("{id}/songs")]
        public async Task<IActionResult> GetAllSongsByPlaylistId(int id)
        {
            var songs = await _playlistService.GetAllSongsByPlaylistIdAsync(id);

            if (songs == null || !songs.Any())
            {
                return NotFound("No songs found for this playlist.");
            }

            return Ok(songs);
        }

        [HttpDelete("{playlistId}/songs/{songId}")]
        public async Task<IActionResult> RemoveSongFromPlaylist(int playlistId, int songId)
        {
            var success = await _playlistService.RemoveSongFromPlaylistAsync(playlistId, songId);

            if (!success)
            {
                return NotFound("Song not found in the playlist.");
            }

            return NoContent();
        }
    }
}
