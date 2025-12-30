namespace ArticleService.Domain.Entities;

public class ArticleStatus
{
    public short Id { get; set; }          // SMALLINT
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}