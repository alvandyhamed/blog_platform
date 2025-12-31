using Microsoft.AspNetCore.Http;

namespace ArticleService.Api.Models;

public class UploadMediaRequest
{
    public IFormFile File { get; set; } = default!;
}