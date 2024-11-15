using MusicPlayerAPI.Models.Dtos;
using MusicPlayerAPI.Models.Entities;

namespace MusicPlayerAPI.Services
{
    public interface ISongService
    {
        FileStream Stream(string filename);
        Task<List<Song>> GetAll();
        Task UploadSong(SongDto songDto, IFormFile file);
        Task<bool?> UpdateSong(int id, SongDto songDto);
        Task DeleteSong(int id);
    }
}