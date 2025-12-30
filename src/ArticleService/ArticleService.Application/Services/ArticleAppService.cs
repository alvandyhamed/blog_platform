using ArticleService.Application.Abstractions;
using ArticleService.Application.Dtos;
using ArticleService.Domain.Entities;

namespace ArticleService.Application.Services;

public class ArticleAppService : IArticleService
{
    private readonly IArticleRepository _repository;

    // Draft = 1, PendingReview = 2, Published = 3, Rejected = 4 (طبق اسکریپت SQL قبلی)
    private const short PendingReviewStatusId = 2;

    public ArticleAppService(IArticleRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> CreateArticleAsync(CreateArticleRequest request, Guid authorId, CancellationToken cancellationToken = default)
    {
        // محاسبه اسلاگ
        var slug = GenerateSlug(request.Title);

        // محاسبه تقریبی زمان مطالعه (200 کلمه در دقیقه)
        var readTime = CalculateReadTimeMinutes(request.ContentMd);

        var article = new Article
        {
            AuthorId = authorId,
            Title = request.Title,
            Slug = slug,
            Summary = request.Summary,
            ContentMd = request.ContentMd,
            HeaderImageUrl = request.HeaderImageUrl,
            StatusId = PendingReviewStatusId,
            ReadTimeMinutes = readTime,
            MetaTitle = request.MetaTitle,
            MetaDescription = request.MetaDescription,
            Keywords = request.Keywords,
            CreatedAt = DateTimeOffset.UtcNow,
            PublishedAt = null
        };

        return await _repository.CreateAsync(article, cancellationToken);
    }

    public async Task<ArticleDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var article = await _repository.GetByIdAsync(id, cancellationToken);
        if (article is null)
            return null;

        return new ArticleDto
        {
            Id = article.Id,
            AuthorId = article.AuthorId,
            Title = article.Title,
            Slug = article.Slug,
            Summary = article.Summary,
            ContentMd = article.ContentMd,
            HeaderImageUrl = article.HeaderImageUrl,
            StatusId = article.StatusId,
            ReadTimeMinutes = article.ReadTimeMinutes,
            MetaTitle = article.MetaTitle,
            MetaDescription = article.MetaDescription,
            Keywords = article.Keywords,
            CreatedAt = article.CreatedAt,
            PublishedAt = article.PublishedAt
        };
    }

    private static string GenerateSlug(string title)
    {
        // خیلی ساده: lowercase + trim + جایگزینی space با dash
        var slug = title.Trim().ToLowerInvariant();

        // حروف غیر مجاز رو حذف ساده
        slug = new string(slug
            .Select(ch => char.IsLetterOrDigit(ch) ? ch : '-')
            .ToArray());

        // چندتا - پشت سر هم رو یکی می‌کنیم
        while (slug.Contains("--"))
        {
            slug = slug.Replace("--", "-");
        }

        return slug.Trim('-');
    }

    private static int CalculateReadTimeMinutes(string contentMd)
    {
        if (string.IsNullOrWhiteSpace(contentMd))
            return 1;

        var words = contentMd
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

        var wordCount = words.Length;
        var minutes = (int)Math.Ceiling(wordCount / 200.0);
        return minutes <= 0 ? 1 : minutes;
    }
}