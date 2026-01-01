using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ArticleService.Application.Dtos;

namespace ArticleService.Application.Services
{
    public interface IArticleService
    {
        /// <summary>
        /// ایجاد مقاله جدید توسط نویسنده
        /// </summary>
        Task<Guid> CreateArticleAsync(
            CreateArticleRequest request,
            Guid authorId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// گرفتن جزییات یک مقاله بر اساس Id
        /// (همان چیزی که در صفحه Article Details لازم داریم)
        /// </summary>
        Task<ArticleDto?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// لیست مقالات منتشر شده (Published) با صفحه‌بندی و جستجو
        /// </summary>
        Task<IReadOnlyList<ArticleDto>> GetPublishedAsync(
            int page,
            int pageSize,
            string? search = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// تأیید مقاله توسط ادمین (تغییر وضعیت به Published)
        /// </summary>
        Task<bool> ApproveAsync(
            Guid id,
            CancellationToken cancellationToken = default);
        Task<LikeResultDto> ToggleReactionAsync(
            Guid articleId,
            Guid userId,
            bool isLike,
            CancellationToken cancellationToken = default);

        Task<long> AddCommentAsync(
Guid articleId,
Guid userId,
string content,
CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ArticleCommentDto>> GetCommentsAsync(
            Guid articleId,
            CancellationToken cancellationToken = default);

    }

}