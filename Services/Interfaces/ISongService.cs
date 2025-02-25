using MusicPlayerAPI.Models.Dtos.Request;
using MusicPlayerAPI.Models.Dtos.Response;

namespace MusicPlayerAPI.Services.Interfaces;

public interface ISongService
{
    Task<StatusResult<List<SongRespDto>>> GetAll();
    Task<StatusResult<SongRespDto>> GetById(int id);
    StatusResult<FileStream> Stream(string songPath);
    Task<StatusResult> Upload(SongReqDto songDto, IFormFile audioFile, IFormFile? coverImageFile, int userId);
    Task<StatusResult> Like(int songId, int userId);
    Task<StatusResult> Dislike(int songId, int userId);
    Task<StatusResult> UpdateCoverImage(int songId, int userId);
    Task<StatusResult> Update(int id, SongReqDto songDto, IFormFile? audioFile, IFormFile? coverImageFile, int userId);
    Task<StatusResult> Delete(int id, int userId);
}