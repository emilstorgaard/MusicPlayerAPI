namespace MusicPlayerAPI.Models.Entities;

public class LikedPlaylist
{
    public int UserId { get; set; }
    public int PlaylistId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public required Playlist Playlist { get; set; }
    public required User User { get; set; }
}
