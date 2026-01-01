namespace ArticleService.Domain.Entities;

public class ArticleComment
{
    public long Id { get; set; }
    public Guid ArticleId { get; set; }
    public Guid UserId { get; set; }
    public string Content { get; set; } = default!;
    public long? ParentId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}