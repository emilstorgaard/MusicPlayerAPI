using MusicPlayerAPI.Configurations;
using MusicPlayerAPI.Dtos.Response;
using MusicPlayerAPI.Exceptions;
using MusicPlayerAPI.Helpers;
using MusicPlayerAPI.Repositories.Interfaces;
using MusicPlayerAPI.Services.Interfaces;

namespace MusicPlayerAPI.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;

    public AuthService(IUserRepository userRepository, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
    }

    public async Task<TokenRespDto> Login(string email, string password)
    {
        var user = await _userRepository.GetUserByEmail(email);
        if (user == null || !PasswordHelper.VerifyPassword(password, user.PasswordHash)) throw new UnauthorizedException("Invalid email or password.");

        var token = _tokenService.GenerateToken(user);

        var tokenRespDto = new TokenRespDto
        {
            Token = token
        };

        return tokenRespDto;
    }
}
