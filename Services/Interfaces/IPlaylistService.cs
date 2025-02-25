using MusicPlayerAPI.Models.Dtos.Request;
using MusicPlayerAPI.Models.Dtos.Response;

namespace MusicPlayerAPI.Services.Interfaces;

public interface IPlaylistService
{
    Task<StatusResult<List<PlaylistRespDto>>> GetAll();
    Task<StatusResult<PlaylistRespDto>> GetById(int id);
    Task<StatusResult> Add(PlaylistReqDto playlistDto, IFormFile? coverImageFile, int userId);
    Task<StatusResult> Like(int playlistId, int userId);
    Task<StatusResult> Dislike(int playlistId, int userId);
    Task<StatusResult> UpdateCoverImage(int playlistId, int userId);
    Task<StatusResult> Update(int id, PlaylistReqDto playlistDto, IFormFile? coverImageFile, int userId);
    Task<StatusResult> Delete(int id, int userId);
    Task<StatusResult> AddToPlaylist(int playlistId, int songId, int userId);
    Task<StatusResult<List<SongRespDto>>> GetAllSongsByPlaylistId(int playlistId);
    Task<StatusResult> RemoveFromPlaylist(int playlistId, int songId, int userId);
}