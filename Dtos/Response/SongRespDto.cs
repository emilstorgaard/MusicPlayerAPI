namespace MusicPlayerAPI.Dtos.Response;

public class SongRespDto
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Artist { get; set; }
    public required string AudioFilePath { get; set; }
    public required string CoverImagePath { get; set; }
    public bool IsLiked { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
