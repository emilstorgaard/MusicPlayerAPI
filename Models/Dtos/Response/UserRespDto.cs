namespace MusicPlayerAPI.Models.Dtos.Response;

public class UserRespDto
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
