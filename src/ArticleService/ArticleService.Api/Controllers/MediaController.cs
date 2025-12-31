using ArticleService.Application.Abstractions;
using ArticleService.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace ArticleService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MediaController : ControllerBase
{
    private readonly IMediaStorageService _mediaStorage;

    public MediaController(IMediaStorageService mediaStorage)
    {
        _mediaStorage = mediaStorage;
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(20_000_000)] // ~20MB
    public async Task<IActionResult> Upload(
        [FromForm] UploadMediaRequest request,
        CancellationToken cancellationToken)
    {
        var file = request.File;

        if (file == null || file.Length == 0)
            return BadRequest("فایل خالی است");

        await using var stream = file.OpenReadStream();

        var url = await _mediaStorage.UploadAsync(
            stream,
            file.FileName,
            file.ContentType ?? "application/octet-stream",
            cancellationToken);

        return Ok(new { url });
    }
}