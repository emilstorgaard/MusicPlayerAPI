namespace MusicPlayerAPI.Models;

public class LikedPlaylist
{
    public int UserId { get; set; }
    public int PlaylistId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Playlist Playlist { get; set; } = null!;
    public User User { get; set; } = null!;
}
