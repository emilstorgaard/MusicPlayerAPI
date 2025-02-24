namespace MusicPlayerAPI.Models.Dtos;

public class PlaylistRespDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string CoverImagePath { get; set; }
    public bool IsLiked { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
