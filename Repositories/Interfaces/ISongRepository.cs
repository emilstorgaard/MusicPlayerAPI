using MusicPlayerAPI.Models;

namespace MusicPlayerAPI.Repositories.Interfaces;

public interface ISongRepository
{
    Task<Song?> GetSongById(int id);
    Task<bool> SongExists(string title, string artist);
    Task AddSong(Song song);
    Task<LikedSong?> GetLikedSongByUser(int songId, int userId);
    Task<List<int>> GetLikedSongIdsByUser(int userId);
    Task<List<int>> GetLikedSongIdsFromPlaylist(int playlistId, int userId);
    Task<bool> IsSongLikedByUser(int songId, int userId);
    Task LikeSong(LikedSong likedSong);
    Task DislikeSong(LikedSong likedSong);
    Task UpdateSong(Song song);
    Task DeleteSong(Song song);

}