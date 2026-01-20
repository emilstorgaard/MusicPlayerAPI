using MusicPlayerAPI.Dtos.Response;
using MusicPlayerAPI.Entities;

namespace MusicPlayerAPI.Mappers;

public static class PlaylistMapper
{
    public static PlaylistRespDto MapToDto(Playlist playlist, bool isLiked)
    {
        return new PlaylistRespDto
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
