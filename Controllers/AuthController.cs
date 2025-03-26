using Microsoft.AspNetCore.Mvc;
using MusicPlayerAPI.Dtos.Request;
using MusicPlayerAPI.Dtos.Response;
using MusicPlayerAPI.Services.Interfaces;

namespace MusicPlayerAPI.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<TokenRespDto>> Login([FromForm] LoginReqDto loginReqDto)
    {
        var result = await _authService.Login(loginReqDto.Email, loginReqDto.Password);
        return result;
    }
}
