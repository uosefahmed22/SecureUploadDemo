using Microsoft.Extensions.Options;
using SecureUploadDemo.Settings;

namespace SecureUploadDemo.Helpers;

/// <summary>
/// Handles all file validation checks (Security Layer)
/// 
/// Step 1: File Size Limit          → Prevent DoS attacks
/// Step 2: Extension Validation     → First layer of defense
/// Step 3: Magic Bytes Validation   → Verify actual file content
/// Step 4: Generate Safe File Name  → Prevent Path Traversal
/// </summary>
public class FileValidator
{
    private readonly UploadSettings _uploadSettings;

    // ============================================
    // Magic Bytes (File Signatures)
    // The first few bytes of a file that identify
    // its actual type, regardless of extension
    // ============================================
    private static readonly Dictionary<string, byte[][]> _imageSignatures = new()
    {
        // JPEG: starts with FF D8 FF
        {
            ".jpg", new[]
            {
                new byte[] { 0xFF, 0xD8, 0xFF }
            }
        },
        {
            ".jpeg", new[]
            {
                new byte[] { 0xFF, 0xD8, 0xFF }
            }
        },
        // PNG: starts with 89 50 4E 47
        {
            ".png", new[]
            {
                new byte[] { 0x89, 0x50, 0x4E, 0x47 }
            }
        },
        // WebP: starts with 52 49 46 46 (RIFF)
        {
            ".webp", new[]
            {
                new byte[] { 0x52, 0x49, 0x46, 0x46 }
            }
        }
    };

    public FileValidator(IOptions<UploadSettings> uploadSettings)
    {
        _uploadSettings = uploadSettings.Value;
    }

    /// <summary>
    /// Runs all validation checks on the uploaded file.
    /// Returns (isValid, errorMessage)
    /// </summary>
    public (bool IsValid, string? ErrorMessage) Validate(IFormFile file)
    {
        // =============================================
        // Step 1: Check File Size
        // Prevent DoS — someone uploading a 2GB file
        // =============================================
        if (file.Length == 0)
            return (false, "File is empty.");

        if (file.Length > _uploadSettings.MaxFileSizeInBytes)
            return (false, $"File size exceeds the maximum allowed size of {_uploadSettings.MaxFileSizeInMB}MB.");

        // =============================================
        // Step 2: Check File Extension
        // First layer of defense (but NOT enough alone!)
        // Someone could rename malware.php → image.jpg
        // =============================================
        var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();

        if (string.IsNullOrEmpty(extension))
            return (false, "File has no extension.");

        if (!_uploadSettings.AllowedExtensions.Contains(extension))
            return (false, $"Extension '{extension}' is not allowed. Allowed: {string.Join(", ", _uploadSettings.AllowedExtensions)}");

        // =============================================
        // Step 3: Check Magic Bytes (File Signature)
        // Read the first bytes of the file to verify
        // it's ACTUALLY an image, not just renamed
        // =============================================
        if (!IsValidMagicBytes(file, extension))
            return (false, "File content does not match its extension. The file may be corrupted or tampered with.");

        return (true, null);
    }

    /// <summary>
    /// Step 4: Generate a safe filename using GUID.
    /// Never use the original filename — prevents Path Traversal.
    /// Example attack: "../../appsettings.json" as filename
    /// </summary>
    public static string GenerateSafeFileName(string extension)
    {
        return $"{Guid.NewGuid()}{extension}";
    }

    /// <summary>
    /// Reads the first bytes of the file and compares them
    /// against known image file signatures (Magic Bytes)
    /// </summary>
    private bool IsValidMagicBytes(IFormFile file, string extension)
    {
        if (!_imageSignatures.TryGetValue(extension, out var signatures))
            return false;

        using var reader = new BinaryReader(file.OpenReadStream());

        // Read enough bytes to check the longest signature
        var maxLength = signatures.Max(s => s.Length);
        var headerBytes = reader.ReadBytes(maxLength);

        if (headerBytes.Length < maxLength)
            return false;

        // Check if the file starts with any of the valid signatures
        return signatures.Any(signature =>
            headerBytes.Take(signature.Length).SequenceEqual(signature));
    }
}
