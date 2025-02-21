using System.Text.Json.Serialization;

namespace MusicPlayerAPI.Models.Entities;

public class User
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    [JsonIgnore]
    public ICollection<Song> Songs { get; set; } = new List<Song>();

    [JsonIgnore]
    public ICollection<Playlist> Playlists { get; set; } = new List<Playlist>();

    [JsonIgnore]
    public ICollection<LikedSong> LikedSongs { get; set; } = new List<LikedSong>();

    [JsonIgnore]
    public ICollection<LikedPlaylist> LikedPlaylists { get; set; } = new List<LikedPlaylist>();
}
