using Microsoft.EntityFrameworkCore;
using MusicPlayerAPI.Data;
using MusicPlayerAPI.Mappers;
using MusicPlayerAPI.Models.Dtos;
using MusicPlayerAPI.Models.Entities;

namespace MusicPlayerAPI.Services;

public class UserService
{
    public readonly ApplicationDbContext _dbContext;

    public UserService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<StatusResult<List<UserResDto>>> GetAll()
    {
        var users = await _dbContext.Users.ToListAsync();

        if (users == null || !users.Any()) return StatusResult<List<UserResDto>>.Failure(404, "No users found.");

        var userDtos = users.Select(u => UserMapper.MapToDto(u)).ToList();
        return StatusResult<List<UserResDto>>.Success(userDtos, 200);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<StatusResult<UserResDto>> AddUser(UserReqDto userReqDto)
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(userReqDto.Password);

        var user = new User
        {
            Email = userReqDto.Email,
            PasswordHash = passwordHash,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();
        return StatusResult<UserResDto>.Success(UserMapper.MapToDto(user), 201);
    }
}
