using ArticleService.Domain.Entities;

namespace ArticleService.Application.Abstractions;

public interface IArticleImageRepository
{
    Task AddRangeAsync(IEnumerable<ArticleImage> images, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ArticleImage>> GetByArticleIdAsync(Guid articleId, CancellationToken cancellationToken = default);
}