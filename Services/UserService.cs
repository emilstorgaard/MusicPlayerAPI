using MusicPlayerAPI.Dtos.Request;
using MusicPlayerAPI.Dtos.Response;
using MusicPlayerAPI.Exceptions;
using MusicPlayerAPI.Helpers;
using MusicPlayerAPI.Mappers;
using MusicPlayerAPI.Entities;
using MusicPlayerAPI.Repositories.Interfaces;
using MusicPlayerAPI.Services.Interfaces;

namespace MusicPlayerAPI.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPlaylistRepository _playlistRepository;
    private readonly ISongRepository _songRepository;

    public UserService(IUserRepository userRepository, IPlaylistRepository playlistRepository, ISongRepository songRepository)
    {
        _userRepository = userRepository;
        _playlistRepository = playlistRepository;
        _songRepository = songRepository;
    }

    public async Task<List<UserRespDto>> GetAll()
    {
        var users = await _userRepository.GetAllUsers();
        if (!users.Any()) throw new NotFoundException("No users found.");

        var userDtos = users.Select(UserMapper.MapToDto).ToList();
        return userDtos;
    }

    public async Task<UserRespDto> GetUser(int userId)
    {
        var user = await _userRepository.GetUserById(userId);
        if (user == null) throw new NotFoundException("User not found.");

        var userDto = UserMapper.MapToDto(user);
        return userDto;
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

    public async Task Delete(int userId)
    {
        var user = await _userRepository.GetUserById(userId);
        if (user == null) throw new NotFoundException("User not found.");

        await _playlistRepository.DeleteLikedPlaylists(userId);
        await _songRepository.DeleteLikedSongs(userId);
        await _userRepository.Delete(user);
    }
}
