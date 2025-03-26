using MusicPlayerAPI.Dtos.Request;
using MusicPlayerAPI.Dtos.Response;
using MusicPlayerAPI.Exceptions;
using MusicPlayerAPI.Helpers;
using MusicPlayerAPI.Mappers;
using MusicPlayerAPI.Models;
using MusicPlayerAPI.Repositories.Interfaces;
using MusicPlayerAPI.Services.Interfaces;

namespace MusicPlayerAPI.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<List<UserRespDto>> GetAll()
    {
        var users = await _userRepository.GetAllUsers();
        if (!users.Any()) throw new NotFoundException("No users found.");

        var userDtos = users.Select(UserMapper.MapToDto).ToList();
        return userDtos;
    }

    public async Task AddUser(UserReqDto userReqDto)
    {
        var existingUser = await _userRepository.GetUserByEmail(userReqDto.Email);
        if (existingUser != null) throw new ConflictException("User with this email already exists.");

        var passwordHash = PasswordHelper.HashPassword(userReqDto.Password);

        var user = new User
        {
            Email = userReqDto.Email,
            PasswordHash = passwordHash,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        await _userRepository.AddUser(user);
    }
}
