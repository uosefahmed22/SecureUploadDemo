using Microsoft.Extensions.Options;
using SecureUploadDemo.Settings;

namespace SecureUploadDemo.Helpers;

public class FileValidator
{
    private readonly UploadSettings _uploadSettings;

    private static readonly Dictionary<string, byte[][]> _imageSignatures = new()
    {
        { ".jpg", new[] { new byte[] { 0xFF, 0xD8, 0xFF } } },
        { ".jpeg", new[] { new byte[] { 0xFF, 0xD8, 0xFF } } },
        { ".png", new[] { new byte[] { 0x89, 0x50, 0x4E, 0x47 } } },
        { ".webp", new[] { new byte[] { 0x52, 0x49, 0x46, 0x46 } } }
    };

    public FileValidator(IOptions<UploadSettings> uploadSettings)
    {
        _uploadSettings = uploadSettings.Value;
    }

    public (bool IsValid, string? ErrorMessage) Validate(IFormFile file)
    {
        if (file.Length == 0)
            return (false, "File is empty.");

        if (file.Length > _uploadSettings.MaxFileSizeInBytes)
            return (false, $"File size exceeds the maximum allowed size of {_uploadSettings.MaxFileSizeInMB}MB.");

        var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();

        if (string.IsNullOrEmpty(extension))
            return (false, "File has no extension.");

        if (!_uploadSettings.AllowedExtensions.Contains(extension))
            return (false, $"Extension '{extension}' is not allowed. Allowed: {string.Join(", ", _uploadSettings.AllowedExtensions)}");

        if (!IsValidMagicBytes(file, extension))
            return (false, "File content does not match its extension.");

        return (true, null);
    }

    public static string GenerateSafeFileName(string extension)
    {
        return $"{Guid.NewGuid()}{extension}";
    }

    private bool IsValidMagicBytes(IFormFile file, string extension)
    {
        if (!_imageSignatures.TryGetValue(extension, out var signatures))
            return false;

        using var reader = new BinaryReader(file.OpenReadStream());
        var maxLength = signatures.Max(s => s.Length);
        var headerBytes = reader.ReadBytes(maxLength);

        if (headerBytes.Length < maxLength)
            return false;

        return signatures.Any(signature =>
            headerBytes.Take(signature.Length).SequenceEqual(signature));
    }
}
