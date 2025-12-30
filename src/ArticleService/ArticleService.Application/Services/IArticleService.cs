using ArticleService.Application.Dtos;

namespace ArticleService.Application.Services;

public interface IArticleService
{
    Task<Guid> CreateArticleAsync(CreateArticleRequest request, Guid authorId, CancellationToken cancellationToken = default);
    Task<ArticleDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ArticleDto>> GetPublishedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<bool> ApproveAsync(Guid id, CancellationToken cancellationToken = default);
}