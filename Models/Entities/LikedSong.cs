namespace MusicPlayerAPI.Models.Entities;

public class LikedSong
{
    public int UserId { get; set; }
    public int SongId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public required Song Song { get; set; }
    public required User User { get; set; }
}
