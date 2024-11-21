using System.Text.Json.Serialization;

namespace MusicPlayerAPI.Models.Entities
{
    public class Song
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string FilePath { get; set; }

        // Add a collection of PlaylistSong to represent the many-to-many relationship
        [JsonIgnore]
        public ICollection<PlaylistSong> PlaylistSongs { get; set; } = new List<PlaylistSong>();
    }
}
