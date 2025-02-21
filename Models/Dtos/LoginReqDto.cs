namespace MusicPlayerAPI.Models.Dtos;

public class LoginReqDto
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}
