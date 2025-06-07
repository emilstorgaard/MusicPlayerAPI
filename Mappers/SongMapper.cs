using MusicPlayerAPI.Dtos.Response;
using MusicPlayerAPI.Models;

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
            Duration = song.Duration,
            AudioFilePath = song.AudioFilePath,
            CoverImagePath = song.CoverImagePath,
            IsLiked = isLiked,
            CreatedAtUtc = song.CreatedAtUtc,
            UpdatedAtUtc = song.UpdatedAtUtc
        };
    }
}
