using MusicPlayerAPI.Models.Dtos.Request;
using MusicPlayerAPI.Models.Dtos.Response;
using MusicPlayerAPI.Models.Entities;

namespace MusicPlayerAPI.Services.Interfaces;

public interface IUserService
{
    Task<StatusResult<List<UserRespDto>>> GetAll();
    Task<User?> GetUserByEmailAsync(string email);
    Task<StatusResult<UserRespDto>> AddUser(UserReqDto userReqDto);
}