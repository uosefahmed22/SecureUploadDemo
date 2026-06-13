namespace SecureUploadDemo.Settings;

public class UploadSettings
{
    public string[] AllowedExtensions { get; set; } = [".jpg", ".jpeg", ".png", ".webp"];
    public int MaxFileSizeInMB { get; set; } = 5;

    /// <summary>
    /// Max file size in bytes (calculated from MaxFileSizeInMB)
    /// </summary>
    public long MaxFileSizeInBytes => MaxFileSizeInMB * 1024L * 1024L;
}
