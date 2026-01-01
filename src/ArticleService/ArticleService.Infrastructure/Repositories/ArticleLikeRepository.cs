using System.Data;
using ArticleService.Application.Abstractions;
using ArticleService.Infrastructure.Abstractions;
using Dapper;

namespace ArticleService.Infrastructure.Repositories;

public sealed class ArticleLikeRepository : IArticleLikeRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ArticleLikeRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<(int Likes, int Dislikes)> ToggleAsync(
        Guid articleId,
        Guid userId,
        bool isLike,
        CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();

        // آیا قبلاً این یوزر روی این مقاله رأی داده؟
        var existing = await conn.QuerySingleOrDefaultAsync<dynamic>(
            @"SELECT id, is_like 
              FROM article_likes 
              WHERE article_id = @ArticleId AND user_id = @UserId",
            new { ArticleId = articleId, UserId = userId }, tx);

        if (existing == null)
        {
            // اولين بار
            await conn.ExecuteAsync(
                @"INSERT INTO article_likes (article_id, user_id, is_like, created_at)
                  VALUES (@ArticleId, @UserId, @IsLike, now())",
                new { ArticleId = articleId, UserId = userId, IsLike = isLike }, tx);
        }
        else if ((bool)existing.is_like == isLike)
        {
            // همان رأی را دوباره زده → حذف (toggle off)
            await conn.ExecuteAsync(
                @"DELETE FROM article_likes WHERE id = @Id",
                new { Id = (long)existing.id }, tx);
        }
        else
        {
            // تغییر رأی از like به dislike یا بالعکس
            await conn.ExecuteAsync(
                @"UPDATE article_likes 
                  SET is_like = @IsLike 
                  WHERE id = @Id",
                new { Id = (long)existing.id, IsLike = isLike }, tx);
        }

        // شمارش
        var counts = await conn.QuerySingleAsync<(int Likes, int Dislikes)>(
            @"SELECT
                COALESCE(SUM(CASE WHEN is_like THEN 1 ELSE 0 END), 0) AS Likes,
                COALESCE(SUM(CASE WHEN NOT is_like THEN 1 ELSE 0 END), 0) AS Dislikes
              FROM article_likes
              WHERE article_id = @ArticleId",
            new { ArticleId = articleId }, tx);

        tx.Commit();
        return counts;
    }
}