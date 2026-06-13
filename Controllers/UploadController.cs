using Microsoft.AspNetCore.Mvc;
using SecureUploadDemo.Helpers;
using SecureUploadDemo.Services;

namespace SecureUploadDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UploadController : ControllerBase
{
    private readonly FileValidator _fileValidator;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly ILogger<UploadController> _logger;

    public UploadController(
        FileValidator fileValidator,
        ICloudinaryService cloudinaryService,
        ILogger<UploadController> logger)
    {
        _fileValidator = fileValidator;
        _cloudinaryService = cloudinaryService;
        _logger = logger;
    }

    [HttpPost("profile-picture")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> UploadProfilePicture(IFormFile file)
    {
        if (file is null)
            return BadRequest(new { error = "No file was provided." });

        var (isValid, errorMessage) = _fileValidator.Validate(file);
        if (!isValid)
        {
            return BadRequest(new { error = errorMessage });
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var safeFileName = FileValidator.GenerateSafeFileName(extension);

        try
        {
            using var stream = file.OpenReadStream();
            var imageUrl = await _cloudinaryService.UploadImageAsync(stream, safeFileName);

            return Ok(new
            {
                message = "File uploaded successfully!",
                url = imageUrl,
                fileName = safeFileName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file {FileName}", safeFileName);
            return StatusCode(500, new { error = "Failed to upload file to cloud storage." });
        }
    }
}
