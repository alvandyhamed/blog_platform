using System.Data;
using ArticleService.Application.Abstractions;
using ArticleService.Domain.Entities;
using ArticleService.Infrastructure.Abstractions;
using Dapper;

namespace ArticleService.Infrastructure.Repositories;

public sealed class ArticleCommentRepository : IArticleCommentRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ArticleCommentRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<long> AddAsync(ArticleComment comment, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO article_comments
                (article_id, user_id, content, parent_id, created_at)
            VALUES
                (@ArticleId, @UserId, @Content, @ParentId, @CreatedAt)
            RETURNING id;";

        using var conn = _connectionFactory.CreateConnection();
        conn.Open();

        var id = await conn.ExecuteScalarAsync<long>(sql, new
        {
            comment.ArticleId,
            comment.UserId,
            comment.Content,
            comment.ParentId,
            comment.CreatedAt
        });

        return id;
    }

    public async Task<IReadOnlyList<ArticleComment>> GetByArticleIdAsync(
        Guid articleId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT id, article_id AS ArticleId, user_id AS UserId,
                   content, parent_id AS ParentId,
                   created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM article_comments
            WHERE article_id = @ArticleId
            ORDER BY created_at ASC;";

        using var conn = _connectionFactory.CreateConnection();
        conn.Open();

        var result = await conn.QueryAsync<ArticleComment>(sql, new { ArticleId = articleId });
        return result.ToList();
    }
}