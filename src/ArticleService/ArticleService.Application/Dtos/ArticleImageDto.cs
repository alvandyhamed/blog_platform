namespace ArticleService.Application.Dtos;

public sealed class ArticleImageDto
{
    public string ImageUrl { get; set; } = null!;
    public string? Caption { get; set; }
    public string? AltText { get; set; }
    public int SortOrder { get; set; }
}