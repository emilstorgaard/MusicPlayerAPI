using MusicPlayerAPI.Models.Dtos;
using MusicPlayerAPI.Models.Entities;

namespace MusicPlayerAPI.Mappers;

public static class UserMapper
{
    public static UserResDto MapToDto(User user)
    {
        return new UserResDto
        {
            Id = user.Id,
            Email = user.Email,
            CreatedAtUtc = user.CreatedAtUtc,
            UpdatedAtUtc = user.UpdatedAtUtc
        };
    }
}
