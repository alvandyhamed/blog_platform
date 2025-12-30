using System.Data;
using ArticleService.Application.Abstractions;
using ArticleService.Domain.Entities;
using ArticleService.Infrastructure.Abstractions;
using Dapper;

namespace ArticleService.Infrastructure.Repositories;

public class ArticleRepository : IArticleRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ArticleRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Guid> CreateAsync(Article article, CancellationToken cancellationToken = default)
    {
        const string sql = @"
INSERT INTO articles (
    id,
    author_id,
    title,
    slug,
    summary,
    content_md,
    header_image_url,
    status_id,
    read_time_minutes,
    meta_title,
    meta_description,
    keywords,
    created_at,
    updated_at,
    published_at
)
VALUES (
    @Id,
    @AuthorId,
    @Title,
    @Slug,
    @Summary,
    @ContentMd,
    @HeaderImageUrl,
    @StatusId,
    @ReadTimeMinutes,
    @MetaTitle,
    @MetaDescription,
    @Keywords,
    @CreatedAt,
    @UpdatedAt,
    @PublishedAt
);";

        if (article.Id == Guid.Empty)
            article.Id = Guid.NewGuid();

        if (article.CreatedAt == default)
            article.CreatedAt = DateTimeOffset.UtcNow;

        using var conn = _connectionFactory.CreateConnection();
        conn.Open(); // 🔹 به‌جای OpenAsync

        await conn.ExecuteAsync(new CommandDefinition(
            sql,
            parameters: article,
            cancellationToken: cancellationToken
        ));

        return article.Id;
    }

    public async Task<Article?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT
    id,
    author_id          AS AuthorId,
    title,
    slug,
    summary,
    content_md         AS ContentMd,
    header_image_url   AS HeaderImageUrl,
    status_id          AS StatusId,
    read_time_minutes  AS ReadTimeMinutes,
    meta_title         AS MetaTitle,
    meta_description   AS MetaDescription,
    keywords,
    created_at         AS CreatedAt,
    updated_at         AS UpdatedAt,
    published_at       AS PublishedAt
FROM articles
WHERE id = @Id;";

        using var conn = _connectionFactory.CreateConnection();
        conn.Open(); // 🔹

        var article = await conn.QuerySingleOrDefaultAsync<Article>(
            new CommandDefinition(
                sql,
                new { Id = id },
                cancellationToken: cancellationToken
            ));

        return article;
    }
}