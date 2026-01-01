using ArticleService.Domain.Entities;

namespace ArticleService.Application.Abstractions;

public interface IArticleLikeRepository
{
    Task<(int Likes, int Dislikes)> ToggleAsync(
        Guid articleId,
        Guid userId,
        bool isLike,
        CancellationToken cancellationToken = default);
}