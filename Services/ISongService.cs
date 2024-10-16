namespace MusicPlayerAPI.Services
{
    public interface ISongService
    {
        FileStream Stream(string filename);
        List<string> GetAllSongs();
        Task UploadSong(IFormFile file);
    }
}