using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicPlayerAPI.Models.Dtos;
using MusicPlayerAPI.Services;

namespace MusicPlayerAPI.Controllers;

[Route("api/users")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;

    public UsersController(UserService userService)
    {
        _userService = userService;
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var result = await _userService.GetAll();
        if (result.Status != 200) return StatusCode(result.Status, result.Message);

        return Ok(result.Data);
    }

    [HttpPost("register")]
    public async Task<IActionResult> AddUser([FromForm] UserReqDto userReqDto)
    {
        var existingUser = await _userService.GetUserByEmailAsync(userReqDto.Email);
        if (existingUser != null) return StatusCode(400, "User with this email already exists.");

        var result = await _userService.AddUser(userReqDto);
        if (result.Status != 201) return StatusCode(result.Status, result.Message);

        return StatusCode(result.Status, result.Message);
    }
}
