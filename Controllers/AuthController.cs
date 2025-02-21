using Microsoft.AspNetCore.Mvc;
using MusicPlayerAPI.Models.Dtos;
using MusicPlayerAPI.Services;

namespace MusicPlayerAPI.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromForm] LoginReqDto loginReqDto)
    {
        var result = await _authService.Login(loginReqDto.Email, loginReqDto.Password);
        if (result.Status != 200) return StatusCode(result.Status, result.Message);

        return Ok(result.Data);
    }
}
