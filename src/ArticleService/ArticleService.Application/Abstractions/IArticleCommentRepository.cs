using ArticleService.Domain.Entities;

namespace ArticleService.Application.Abstractions;

public interface IArticleCommentRepository
{
    Task<long> AddAsync(ArticleComment comment, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ArticleComment>> GetByArticleIdAsync(
        Guid articleId,
        CancellationToken cancellationToken = default);
}