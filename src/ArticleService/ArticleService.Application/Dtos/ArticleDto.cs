namespace ArticleService.Application.Dtos;

public class ArticleDto
{
    public Guid Id { get; set; }
    public Guid AuthorId { get; set; }

    public string Title { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string? Summary { get; set; }
    public string ContentMd { get; set; } = default!;

    public string? HeaderImageUrl { get; set; }

    public short StatusId { get; set; }
    public int? ReadTimeMinutes { get; set; }

    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string[]? Keywords { get; set; }
    public List<ArticleImageDto> Images { get; set; } = new();

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
}