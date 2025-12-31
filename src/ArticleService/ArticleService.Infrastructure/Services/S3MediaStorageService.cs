using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using ArticleService.Application.Abstractions;
using ArticleService.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace ArticleService.Infrastructure.Services;

public class S3MediaStorageService : IMediaStorageService
{
    private readonly IAmazonS3 _s3;
    private readonly MinioOptions _options;

    public S3MediaStorageService(IAmazonS3 s3, IOptions<MinioOptions> options)
    {
        _s3 = s3;
        _options = options.Value;
    }

    public async Task<string> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        // یه key تمیز بسازیم که پوشه‌وار باشه
        var key = $"{DateTime.UtcNow:yyyy/MM/dd}/{Guid.NewGuid()}_{fileName}";

        var putRequest = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            InputStream = fileStream,
            ContentType = contentType
        };

        await _s3.PutObjectAsync(putRequest, cancellationToken);

        // URL عمومی برمی‌گردونیم
        return $"{_options.PublicBaseUrl}/{key}";
    }
}