using MusicPlayerAPI.Models.Dtos;
using MusicPlayerAPI.Models.Entities;

namespace MusicPlayerAPI.Mappers;

public static class PlaylistMapper
{
    public static PlaylistResDto MapToDto(Playlist playlist, bool isLiked)
    {
        return new PlaylistResDto
        {
            Id = playlist.Id,
            Name = playlist.Name,
            CoverImagePath = playlist.CoverImagePath,
            IsLiked = isLiked,
            CreatedAtUtc = playlist.CreatedAtUtc,
            UpdatedAtUtc = playlist.UpdatedAtUtc
        };
    }
}
