using MusicPlayerAPI.Dtos.Request;
using MusicPlayerAPI.Dtos.Response;

namespace MusicPlayerAPI.Services.Interfaces;

public interface IUserService
{
    Task<List<UserRespDto>> GetAll();
    Task<UserRespDto> GetUser(int userId);
    Task AddUser(UserReqDto userReqDto);
    Task Delete(int userId);
}