namespace MusicPlayerAPI.Entities;

public class LikedSong
{
    public int UserId { get; set; }
    public int SongId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Song Song { get; set; } = null!;
    public User User { get; set; } = null!;
}
