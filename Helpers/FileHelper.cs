namespace MusicPlayerAPI.Helpers
{
    public static class FileHelper
    {
        private const string DefaultCoverImage = "default.jpg";

        public static bool IsValidFile(IFormFile file, string[] allowedExtensions)
        {
            return file.Length == 0 ? false : allowedExtensions.Contains(Path.GetExtension(file.FileName).ToLower());

        }

        public static string SaveFile(IFormFile file, string folderPath)
        {
            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName).ToLower();
            var filePath = Path.Combine(folderPath, fileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            file.CopyTo(stream);
            return filePath;
        }

        public static void DeleteFile(string? filePath)
        {
            if (!string.IsNullOrEmpty(filePath) && !filePath.EndsWith(DefaultCoverImage) && File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public static string GetDefaultCoverImagePath(string folderPath)
        {
            return Path.Combine(folderPath, DefaultCoverImage);
        }
    }
}
