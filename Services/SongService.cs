namespace MusicPlayerAPI.Services
{
    public class SongService : ISongService
    {
        private readonly string _uploadFolderPath;

        public SongService()
        {
            _uploadFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Songs");
            Directory.CreateDirectory(_uploadFolderPath);
        }

        public FileStream Stream(string filename)
        {
            var filePath = Path.Combine(_uploadFolderPath, filename);
            if (File.Exists(filePath))
            {
                return new FileStream(filePath, FileMode.Open, FileAccess.Read);
            }
            else
            {
                return null;
            }
        }

        public List<string> GetAllSongs()
        {
            if (Directory.Exists(_uploadFolderPath))
            {
                return Directory.GetFiles(_uploadFolderPath)
                    .Select(Path.GetFileName)
                    .ToList();
            }
            return new List<string>();
        }

        public async Task UploadSong(IFormFile file)
        {
            var filePath = Path.Combine(_uploadFolderPath, file.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
        }
    }
}
