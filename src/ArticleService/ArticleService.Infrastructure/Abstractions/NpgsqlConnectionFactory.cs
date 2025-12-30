using System.Data;
using ArticleService.Infrastructure.Abstractions;
using Npgsql;

namespace ArticleService.Infrastructure;

public class NpgsqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public NpgsqlConnectionFactory(string connectionString)
    {
        _connectionString = connectionString
                            ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public IDbConnection CreateConnection()
    {
        var conn = new NpgsqlConnection(_connectionString);
        // می‌تونی اینجا هم Open کنی؛ ولی بهتره Repository تصمیم بگیره
        return conn;
    }
}