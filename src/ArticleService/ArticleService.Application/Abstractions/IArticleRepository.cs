using ArticleService.Domain.Entities;

namespace ArticleService.Application.Abstractions;

public interface IArticleRepository
{
    Task<Guid> CreateAsync(Article article, CancellationToken cancellationToken = default);
    Task<Article?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}