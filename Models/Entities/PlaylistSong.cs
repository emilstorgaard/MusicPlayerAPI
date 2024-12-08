namespace MusicPlayerAPI.Models.Entities
{
    public class PlaylistSong
    {
        public int PlaylistId { get; set; }
        public int SongId { get; set; }

        public Playlist Playlist { get; set; }
        public Song Song { get; set; }
    }
}
