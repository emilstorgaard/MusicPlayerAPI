using NAudio.Wave;
namespace MusicPlayerAPI.Helpers;

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
        var fullPath = GetFullPath(filePath);
        using var stream = new FileStream(fullPath, FileMode.Create);
        file.CopyTo(stream);
        return filePath;
    }

    public static void DeleteFile(string? filePath)
    {
        if (string.IsNullOrEmpty(filePath)) return;

        var fullFilePath = GetFullPath(filePath);
        if (!fullFilePath.EndsWith(DefaultCoverImage) && File.Exists(fullFilePath))
        {
            File.Delete(fullFilePath);
        }
    }

    public static string GetDefaultCoverImagePath(string folderPath)
    {
        return Path.Combine(folderPath, DefaultCoverImage);
    }

    public static string GetFullPath(string relativePath)
    {
        return Path.Combine(Directory.GetCurrentDirectory(), relativePath);
    }

    public static TimeSpan GetAudioDuration(string filePath)
    {
        using (var reader = new AudioFileReader(filePath))
        {
            return reader.TotalTime;
        }
    }
}
