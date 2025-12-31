using Microsoft.AspNetCore.Http;
namespace ArticleService.Application.Dtos;

public class CreateArticleRequest
{
    public string Title { get; set; } = default!;
    public string? Summary { get; set; }
    public string ContentMd { get; set; } = default!;

    public string? HeaderImageUrl { get; set; }

    // SEO
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }

    // کلمات کلیدی
    public string[]? Keywords { get; set; }
    public List<ArticleImageDto> Images { get; set; } = new();
    // public IFormFile? HeaderImage { get; set; }
}