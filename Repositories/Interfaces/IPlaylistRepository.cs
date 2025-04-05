using MusicPlayerAPI.Models;

namespace MusicPlayerAPI.Repositories.Interfaces;

public interface IPlaylistRepository
{
    Task<List<Playlist>> GetAllPlaylistsByUserId(int userId);
    Task<Playlist?> GetPlaylistById(int id);
    Task<Playlist?> GetExistingPlaylist(string name, int userId);
    Task AddPlaylist(Playlist playlist);
    Task<LikedPlaylist?> GetLikedPlaylistByUser(int playlistId, int userId);
    Task<List<int>> GetLikedPlaylistIdsByUser(int userId);
    Task<bool> IsPlaylistLikedByUser(int playlistId, int userId);
    Task LikePlaylist(LikedPlaylist likedPlaylist);
    Task DislikePlaylist(LikedPlaylist likedPlaylist);
    Task UpdatePlaylist(Playlist playlist);
    Task DeletePlaylist(Playlist playlist);
    Task DeletePlaylistSongs(int playlistId);
    Task DeleteLikedPlaylists(int userId);
    Task<bool> IsSongInPlaylist(int playlistId, int songId);
    Task AddSongToPlaylist(PlaylistSong playlistSong);
    Task<List<Song>> GetSongsByPlaylistId(int playlistId);
    Task<PlaylistSong?> GetPlaylistSong(int playlistId, int songId);
    Task RemoveSongFromPlaylist(PlaylistSong playlistSong);
}