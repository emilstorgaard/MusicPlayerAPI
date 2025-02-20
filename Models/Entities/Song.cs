using System.Text.Json.Serialization;

namespace MusicPlayerAPI.Models.Entities
{
    public class Song
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Artist { get; set; }
        public required string AudioFilePath { get; set; }
        public required string CoverImagePath { get; set; }

        // Add a collection of PlaylistSong to represent the many-to-many relationship
        [JsonIgnore]
        public ICollection<PlaylistSong> PlaylistSongs { get; set; } = new List<PlaylistSong>();
    }
}
