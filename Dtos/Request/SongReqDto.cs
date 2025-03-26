namespace MusicPlayerAPI.Dtos.Request;

public class SongReqDto
{
    public required string Title { get; set; }
    public required string Artist { get; set; }
    public required IFormFile AudioFile { get; set; }
    public IFormFile? CoverImageFile { get; set; }
}
