namespace ArticleService.Domain.Entities;

public class Tag
{
    public long Id { get; set; }
    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
}