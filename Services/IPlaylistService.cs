using MusicPlayerAPI.Models.Dtos;
using MusicPlayerAPI.Models.Entities;

namespace MusicPlayerAPI.Services
{
    public interface IPlaylistService
    {
        Task<List<Playlist>> GetAll();
        Task<bool?> AddPlaylist(PlaylistDto playlistDto);
        Task<bool?> UpdatePlaylist(int id, PlaylistDto playlistDto);
        Task<bool?> DeletePlaylist(int id);
    }
}