namespace ArticleService.Domain.Entities;

public class ArticleLink
{
    public long Id { get; set; }
    public Guid ArticleId { get; set; }

    public string Url { get; set; } = default!;
    public string? LinkText { get; set; }
    public int? SortOrder { get; set; }
    public bool IsExternal { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}