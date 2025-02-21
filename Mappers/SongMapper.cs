using MusicPlayerAPI.Models.Dtos;
using MusicPlayerAPI.Models.Entities;

namespace MusicPlayerAPI.Mappers;

public static class SongMapper
{
    public static SongResDto MapToDto(Song song, bool isLiked)
    {
        return new SongResDto
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
