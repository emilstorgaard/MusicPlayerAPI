using MusicPlayerAPI.Dtos.Response;

namespace MusicPlayerAPI.Services.Interfaces;

public interface IAuthService
{
    Task<TokenRespDto> Login(string email, string password);
}