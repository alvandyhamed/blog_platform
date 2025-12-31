using ArticleService.Domain.Entities;

namespace ArticleService.Application.Abstractions;

public interface IArticleRepository
{
    Task<Guid> CreateAsync(Article article, CancellationToken cancellationToken = default);
    Task<Article?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Article>> GetPublishedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateStatusAsync(
        Guid id,
        short statusId,
        DateTimeOffset? publishedAt,
        CancellationToken cancellationToken = default);


    Task<(IReadOnlyList<Article> Items, int TotalCount)> GetPublishedPagedAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken = default);

    Task<Article?> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default);
}


