namespace MusicPlayerAPI.Entities;

public class PlaylistSong
{
    public int PlaylistId { get; set; }
    public int SongId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Playlist Playlist { get; set; } = null!;
    public Song Song { get; set; } = null!;

}
