namespace ArticleService.Domain.Entities;

public class Article
{
    public Guid Id { get; set; }
    public Guid AuthorId { get; set; }          // از IdentityService (users.id)

    public string Title { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string? Summary { get; set; }
    public string ContentMd { get; set; } = default!;   // متن کامل Markdown

    public string? HeaderImageUrl { get; set; }

    public short StatusId { get; set; }         // FK به article_statuses
    public int? ReadTimeMinutes { get; set; }

    // SEO
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }

    // TEXT[] در Postgres → string[] در دات‌نت
    public string[]? Keywords { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }

}