using MusicPlayerAPI.Models.Dtos.Response;
using MusicPlayerAPI.Models.Entities;

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
