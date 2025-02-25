using MusicPlayerAPI.Models.Dtos.Response;
using MusicPlayerAPI.Models.Entities;

namespace MusicPlayerAPI.Mappers;

public static class SongMapper
{
    public static SongRespDto MapToDto(Song song, bool isLiked)
    {
        return new SongRespDto
        {
            Id = song.Id,
            Title = song.Title,
            Artist = song.Artist,
            AudioFilePath = song.AudioFilePath,
            CoverImagePath = song.CoverImagePath,
            IsLiked = isLiked,
            CreatedAtUtc = song.CreatedAtUtc,
            UpdatedAtUtc = song.UpdatedAtUtc
        };
    }
}
