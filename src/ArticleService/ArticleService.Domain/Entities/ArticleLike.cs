namespace ArticleService.Domain.Entities;

public class ArticleLike
{
    public long Id { get; set; }
    public Guid ArticleId { get; set; }
    public Guid UserId { get; set; }
    public bool IsLike { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}