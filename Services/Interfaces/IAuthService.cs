using MusicPlayerAPI.Models.Dtos.Response;

namespace MusicPlayerAPI.Services.Interfaces;

public interface IAuthService
{
    Task<StatusResult<TokenRespDto>> Login(string email, string password);
}