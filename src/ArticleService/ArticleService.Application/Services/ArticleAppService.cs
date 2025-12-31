using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArticleService.Application.Abstractions;
using ArticleService.Application.Dtos;
using ArticleService.Domain.Entities;

namespace ArticleService.Application.Services
{
    public sealed class ArticleAppService : IArticleService
    {
        private readonly IArticleRepository _repository;
        private readonly IArticleImageRepository _articleImageRepository;

        // Draft = 1, PendingReview = 2, Published = 3, Rejected = 4
        private const short PendingReviewStatusId = 2;
        private const short PublishedStatusId = 3;

        public ArticleAppService(
            IArticleRepository repository,
            IArticleImageRepository articleImageRepository)
        {
            _repository = repository;
            _articleImageRepository = articleImageRepository;
        }

        #region Create

        public async Task<Guid> CreateArticleAsync(
            CreateArticleRequest request,
            Guid authorId,
            CancellationToken cancellationToken = default)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var now = DateTimeOffset.UtcNow;
            var slug = Slugify(request.Title);

            var article = new Article
            {
                Id = Guid.NewGuid(),
                AuthorId = authorId,
                Title = request.Title,
                Slug = slug,
                Summary = request.Summary,
                ContentMd = request.ContentMd,
                HeaderImageUrl = request.HeaderImageUrl,
                StatusId = PendingReviewStatusId,
                ReadTimeMinutes = CalculateReadTime(request.ContentMd),
                MetaTitle = string.IsNullOrWhiteSpace(request.MetaTitle)
                                      ? request.Title
                                      : request.MetaTitle,
                MetaDescription = request.MetaDescription,
                Keywords = request.Keywords,
                CreatedAt = now,
                UpdatedAt = null,
                PublishedAt = null
            };

            // خود این متد INSERT روی جدول articles را انجام می‌دهد
            await _repository.CreateAsync(article, cancellationToken);

            // اگر برای این مقاله عکس‌های محتوا فرستاده شده باشد، در جدول article_images ذخیره کنیم
            if (request.Images is { Count: > 0 })
            {
                var images = request.Images
                    .Select((img, index) => new ArticleImage
                    {
                        ArticleId = article.Id,
                        ImageUrl = img.ImageUrl,
                        Caption = img.Caption,
                        AltText = img.AltText,
                        // ArticleImage.SortOrder = int است ولی در DTO احتمالاً int?؛
                        // پس با ?? مقدار دهی می‌کنیم.
                        SortOrder = img.SortOrder,
                        CreatedAt = now
                    });

                await _articleImageRepository.AddRangeAsync(images, cancellationToken);
            }

            return article.Id;
        }

        #endregion

        #region GetById

        public async Task<ArticleDto?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var article = await _repository.GetByIdAsync(id, cancellationToken);
            if (article is null)
                return null;

            // تصاویر همین مقاله
            var images = await _articleImageRepository
                .GetByArticleIdAsync(article.Id, cancellationToken);

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
                PublishedAt = article.PublishedAt,
                Images = images
                    .OrderBy(i => i.SortOrder)
                    .Select(i => new ArticleImageDto
                    {
                        ImageUrl = i.ImageUrl,
                        Caption = i.Caption,
                        AltText = i.AltText,
                        SortOrder = i.SortOrder ?? 0
                    })
                    .ToList()
            };
        }

        #endregion

        #region GetPublished (list + search)

        public async Task<IReadOnlyList<ArticleDto>> GetPublishedAsync(
            int page,
            int pageSize,
            string? search = null,
            CancellationToken cancellationToken = default)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            // فعلاً امضای IArticleRepository را دست نمی‌زنیم
            var articles = await _repository
                .GetPublishedAsync(page, pageSize, cancellationToken);

            // فیلتر ساده در لایه سرویس
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();

                articles = articles
                    .Where(a =>
                        (!string.IsNullOrEmpty(a.Title) && a.Title.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(a.Summary) && a.Summary.Contains(term, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            var result = new List<ArticleDto>(articles.Count);

            foreach (var a in articles)
            {
                result.Add(new ArticleDto
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
                    // برای لیست، فعلاً تصاویر را نمی‌آوریم (مثل قبل)
                    Images = new List<ArticleImageDto>()
                });
            }

            return result;
        }

        #endregion

        #region Approve

        public async Task<bool> ApproveAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var now = DateTimeOffset.UtcNow;

            // این متد باید در ArticleRepository پیاده‌سازی شده باشد
            return await _repository.UpdateStatusAsync(
                id,
                PublishedStatusId,
                now,
                cancellationToken);
        }

        #endregion

        #region Helpers

        private static int CalculateReadTime(string contentMd)
        {
            if (string.IsNullOrWhiteSpace(contentMd))
                return 1;

            var words = contentMd.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var minutes = (int)Math.Ceiling(words.Length / 200.0); // حدود ۲۰۰ کلمه در دقیقه

            return Math.Max(minutes, 1);
        }

        private static string Slugify(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var slug = value.Trim().ToLowerInvariant();

            // فاصله‌ها → خط تیره
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");
            // حذف کاراکترهای غیر مجاز (حروف لاتین، اعداد و حروف فارسی را نگه می‌داریم)
            slug = System.Text.RegularExpressions.Regex.Replace(
                slug,
                @"[^a-z0-9\-\u0600-\u06FF]",
                string.Empty);

            return slug;
        }

        #endregion
    }
}