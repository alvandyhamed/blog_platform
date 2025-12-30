namespace ArticleService.Domain.Entities;

public class ArticleTag
{
    public Guid ArticleId { get; set; }
    public long TagId { get; set; }
}