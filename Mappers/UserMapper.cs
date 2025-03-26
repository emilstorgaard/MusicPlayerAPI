using MusicPlayerAPI.Dtos.Response;
using MusicPlayerAPI.Models;

namespace MusicPlayerAPI.Mappers;

public static class UserMapper
{
    public static UserRespDto MapToDto(User user)
    {
        return new UserRespDto
        {
            Id = user.Id,
            Email = user.Email,
            CreatedAtUtc = user.CreatedAtUtc,
            UpdatedAtUtc = user.UpdatedAtUtc
        };
    }
}
