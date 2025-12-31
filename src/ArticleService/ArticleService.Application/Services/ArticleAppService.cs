using ArticleService.Application.Abstractions;
using ArticleService.Application.Dtos;
using ArticleService.Domain.Entities;

namespace ArticleService.Application.Services;

public class ArticleAppService : IArticleService
{
    private readonly IArticleRepository _repository;
    private readonly IArticleImageRepository _articleImageRepository;




    // Draft = 1, PendingReview = 2, Published = 3, Rejected = 4 (طبق اسکریپت SQL قبلی)
    private const short PendingReviewStatusId = 2;
    private const short PublishedStatusId = 3;

    public ArticleAppService(IArticleRepository repository, IArticleImageRepository articleImageRepository)
    {
        _repository = repository;
        _articleImageRepository = articleImageRepository;
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
        await _repository.CreateAsync(article, cancellationToken);



        // ۳) ذخیره‌ی عکس‌های متن در جدول article_images
        if (request.Images is { Count: > 0 })
        {
            var now = DateTimeOffset.UtcNow;

            var images = request.Images.Select((img, index) => new ArticleImage
            {
                ArticleId = article.Id,
                ImageUrl = img.ImageUrl,
                Caption = img.Caption,
                AltText = img.AltText,
                SortOrder = index,   // یا img.SortOrder اگر تو DTO داری
                CreatedAt = now
            });

            await _articleImageRepository.AddRangeAsync(images, cancellationToken);
        }

        // ۴) برگردوندن Id مقاله
        return article.Id;
    }

    public async Task<ArticleDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var article = await _repository.GetByIdAsync(id, cancellationToken);
        if (article is null)
            return null;
        var images = await _articleImageRepository.GetByArticleIdAsync(id, cancellationToken);
        var imagesLookup = images.ToLookup(i => i.ArticleId);


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
            Images = images.Select(img => new ArticleImageDto
            {
                ImageUrl = img.ImageUrl,
                Caption = img.Caption,
                AltText = img.AltText,
                SortOrder = img.SortOrder ?? 0
            }).ToList(),
            CreatedAt = article.CreatedAt,
            PublishedAt = article.PublishedAt,

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

    public async Task<IReadOnlyList<ArticleDto>> GetPublishedAsync(
    int page,
    int pageSize,
    CancellationToken cancellationToken = default)
    {
        var articles = await _repository.GetPublishedAsync(page, pageSize, cancellationToken);
        var result = new List<ArticleDto>();
        foreach (var a in articles)
        {
            var images = await _articleImageRepository.GetByArticleIdAsync(a.Id, cancellationToken);
            result.Add(
                new ArticleDto
                {
                    Id = a.Id,
                    AuthorId = a.AuthorId,
                    Title = a.Title,
                    Slug = a.Slug,
                    Summary = a.Summary,
                    ContentMd = a.ContentMd,
                    HeaderImageUrl = a.HeaderImageUrl,
                    StatusId = a.StatusId,
                    ReadTimeMinutes = a.ReadTimeMinutes,
                    MetaTitle = a.MetaTitle,
                    MetaDescription = a.MetaDescription,
                    Keywords = a.Keywords,
                    CreatedAt = a.CreatedAt,
                    PublishedAt = a.PublishedAt,
                    Images = images
                .Select(img => new ArticleImageDto
                {
                    ImageUrl = img.ImageUrl,
                    Caption = img.Caption,
                    AltText = img.AltText,
                    SortOrder = img.SortOrder ?? 0
                })
                .ToList()

                }
            );
        }
        return result;



    }

    public async Task<bool> ApproveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        return await _repository.UpdateStatusAsync(
            id,
            PublishedStatusId,
            now,
            cancellationToken);
    }
}