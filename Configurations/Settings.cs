namespace MusicPlayerAPI.Configurations;

public class Settings
{
    public string UploadAudioFolderPath { get; set; }
    public string UploadImageFolderPath { get; set; }
    public string[] AllowedAudioExtensions { get; set; }
    public string[] AllowedImageExtensions { get; set; }
    public string JwtSecret { get; set; }
    public int JwtExpiryHours { get; set; }
}
