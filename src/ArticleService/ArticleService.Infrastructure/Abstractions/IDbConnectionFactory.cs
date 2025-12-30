using System.Data;

namespace ArticleService.Infrastructure.Abstractions;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}