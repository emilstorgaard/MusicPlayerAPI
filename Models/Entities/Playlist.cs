using System.Text.Json.Serialization;

namespace MusicPlayerAPI.Models.Entities
{
    public class Playlist
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string CoverImagePath { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        // Add a collection of PlaylistSong to represent the many-to-many relationship
        [JsonIgnore]
        public ICollection<PlaylistSong> PlaylistSongs { get; set; } = new List<PlaylistSong>();
    }
}
