namespace MusicPlayerAPI.Models.Entities
{
    public class PlaylistSong
    {
        public int PlaylistId { get; set; }
        public int SongId { get; set; }
        public required Playlist Playlist { get; set; }
        public required Song Song { get; set; }
    }
}
