using MusicPlayerAPI.Models;

namespace MusicPlayerAPI.Repositories.Interfaces;

public interface ISearchRepository
{
    Task<List<Playlist>> GetPlaylistsBySearch(string search);
    Task<List<Song>> GetSongsBySearch(string search);
}