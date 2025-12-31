using System.Data;
using ArticleService.Application.Abstractions;
using ArticleService.Domain.Entities;
using Dapper;
using ArticleService.Infrastructure.Abstractions;

namespace ArticleService.Infrastructure.Repositories;

public sealed class ArticleImageRepository : IArticleImageRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ArticleImageRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task AddRangeAsync(IEnumerable<ArticleImage> images, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO article_images
                (article_id, image_url, caption, alt_text, sort_order, created_at)
            VALUES
                (@ArticleId, @ImageUrl, @Caption, @AltText, @SortOrder, @CreatedAt);";

        using var conn = _connectionFactory.CreateConnection();
        conn.Open();

        foreach (var img in images)
        {
            if (img.CreatedAt == default)
                img.CreatedAt = DateTimeOffset.UtcNow;

            await conn.ExecuteAsync(
                new CommandDefinition(sql, img, cancellationToken: cancellationToken));
        }
    }

    public async Task<IReadOnlyList<ArticleImage>> GetByArticleIdAsync(Guid articleId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT id,
                   article_id   AS ArticleId,
                   image_url    AS ImageUrl,
                   caption      AS Caption,
                   alt_text     AS AltText,
                   sort_order   AS SortOrder,
                   created_at   AS CreatedAt
            FROM article_images
            WHERE article_id = @ArticleId
            ORDER BY sort_order, id;";

        using var conn = _connectionFactory.CreateConnection();
        conn.Open();

        var rows = await conn.QueryAsync<ArticleImage>(
            new CommandDefinition(sql, new { ArticleId = articleId }, cancellationToken: cancellationToken));

        return rows.ToList();
    }
}