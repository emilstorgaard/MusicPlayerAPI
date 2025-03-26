namespace MusicPlayerAPI.Dtos.Request;

public class PlaylistReqDto
{
    public required string Name { get; set; }
    public IFormFile? CoverImageFile { get; set; }
}
