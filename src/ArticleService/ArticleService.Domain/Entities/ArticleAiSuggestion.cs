namespace ArticleService.Domain.Entities;

public class ArticleAiSuggestion
{
    public long Id { get; set; }
    public Guid ArticleId { get; set; }

    public string? SuggestedMetaTitle { get; set; }
    public string? SuggestedMetaDescription { get; set; }
    public string[]? SuggestedKeywords { get; set; }   // TEXT[]
    public string? Model { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}