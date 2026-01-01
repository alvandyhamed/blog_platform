namespace ArticleService.Application.Dtos;

public class ArticleCommentDto
{
    public long Id { get; set; }
    public Guid ArticleId { get; set; }
    public Guid UserId { get; set; }
    public string Content { get; set; } = default!;
    public DateTimeOffset CreatedAt { get; set; }
}