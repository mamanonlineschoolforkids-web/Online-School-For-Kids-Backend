using Domain.Interfaces.Services.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UploadController : ControllerBase
{
    private readonly IFileStorageService _fileStorage;
    private static readonly string[] AllowedImageExts = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private static readonly string[] AllowedVideoExts = { ".mp4", ".mov", ".webm", ".avi" };
    private const long MaxImageSize = 10 * 1024 * 1024;  // 10 MB
    private const long MaxVideoSize = 100 * 1024 * 1024; // 100 MB

    public UploadController(IFileStorageService fileStorage)
    {
        _fileStorage = fileStorage;
    }

    /// <summary>POST /api/upload/feed-media — upload image or video for a feed post</summary>
    [HttpPost("feed-media")]
    public async Task<IActionResult> UploadFeedMedia(
        IFormFile file,
        CancellationToken ct = default)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file provided." });

        var ext = _fileStorage.GetFileExtension(file.FileName);
        bool isImage = AllowedImageExts.Contains(ext);
        bool isVideo = AllowedVideoExts.Contains(ext);

        if (!isImage && !isVideo)
            return BadRequest(new { message = $"File type '{ext}' is not allowed." });

        if (isImage && file.Length > MaxImageSize)
            return BadRequest(new { message = "Image must be under 10 MB." });

        if (isVideo && file.Length > MaxVideoSize)
            return BadRequest(new { message = "Video must be under 100 MB." });

        await using var stream = file.OpenReadStream();
        var url = await _fileStorage.UploadFileAsync(stream, file.FileName, "feed");

        return Ok(new
        {
            url,
            mediaType = isImage ? "image" : "video"
        });
    }
}