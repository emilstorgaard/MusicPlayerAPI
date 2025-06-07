namespace MusicPlayerAPI.Dtos.Response;

public class SearchRespDto
{
    public required List<PlaylistRespDto> Playlists { get; set; }
    public required List<SongRespDto> Songs { get; set; }
}
