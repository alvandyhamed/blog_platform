namespace ArticleService.Domain.Entities;

public class ArticleImage
{
    public long Id { get; set; }            // BIGSERIAL
    public Guid ArticleId { get; set; }

    public string ImageUrl { get; set; } = default!;
    public string? Caption { get; set; }
    public string? AltText { get; set; }
    public int? SortOrder { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}