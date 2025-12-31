namespace ArticleService.Application.Abstractions;

public interface IMediaStorageService
{
    /// <summary>
    /// فایل رو آپلود می‌کنه و URL نهایی رو برمی‌گردونه.
    /// </summary>
    Task<string> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);
}