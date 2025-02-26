using Microsoft.EntityFrameworkCore;
using MusicPlayerAPI.Data;
using MusicPlayerAPI.Helpers;
using MusicPlayerAPI.Mappers;
using MusicPlayerAPI.Models.Dtos.Request;
using MusicPlayerAPI.Models.Dtos.Response;
using MusicPlayerAPI.Models.Entities;
using MusicPlayerAPI.Services.Interfaces;

namespace MusicPlayerAPI.Services;

public class UserService : IUserService
{
    public readonly ApplicationDbContext _dbContext;

    public UserService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<StatusResult<List<UserRespDto>>> GetAll()
    {
        var users = await _dbContext.Users.ToListAsync();

        if (!users.Any()) return StatusResult<List<UserRespDto>>.Failure(404, "No users found.");

        var userDtos = users.Select(UserMapper.MapToDto).ToList();
        return StatusResult<List<UserRespDto>>.Success(userDtos, 200);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<StatusResult<UserRespDto>> AddUser(UserReqDto userReqDto)
    {
        var passwordHash = PasswordHelper.HashPassword(userReqDto.Password);

        var user = new User
        {
            Email = userReqDto.Email,
            PasswordHash = passwordHash,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        return StatusResult<UserRespDto>.Success(UserMapper.MapToDto(user), 201);
    }
}
