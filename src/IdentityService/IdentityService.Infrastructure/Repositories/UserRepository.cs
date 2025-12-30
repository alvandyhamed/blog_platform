using System.Data;
using Dapper;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Data;

namespace IdentityService.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT id, google_id AS GoogleId, email, display_name AS DisplayName,
                   avatar_url AS AvatarUrl, is_active AS IsActive,
                   created_at AS CreatedAt, last_login_at AS LastLoginAt
            FROM users
            WHERE id = @id;";

        return await conn.QuerySingleOrDefaultAsync<User>(sql, new { id });
    }

    public async Task<User?> GetByGoogleIdAsync(string googleId)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT id, google_id AS GoogleId, email, display_name AS DisplayName,
                   avatar_url AS AvatarUrl, is_active AS IsActive,
                   created_at AS CreatedAt, last_login_at AS LastLoginAt
            FROM users
            WHERE google_id = @googleId;";

        return await conn.QuerySingleOrDefaultAsync<User>(sql, new { googleId });
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT id, google_id AS GoogleId, email, display_name AS DisplayName,
                   avatar_url AS AvatarUrl, is_active AS IsActive,
                   created_at AS CreatedAt, last_login_at AS LastLoginAt
            FROM users
            WHERE email = @email;";

        return await conn.QuerySingleOrDefaultAsync<User>(sql, new { email });
    }

    public async Task AddAsync(User user)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO users
                (id, google_id, email, display_name, avatar_url,
                 is_active, created_at, last_login_at)
            VALUES
                (@Id, @GoogleId, @Email, @DisplayName, @AvatarUrl,
                 @IsActive, @CreatedAt, @LastLoginAt);";

        await conn.ExecuteAsync(sql, user);
    }

    public async Task UpdateAsync(User user)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE users
            SET email        = @Email,
                display_name = @DisplayName,
                avatar_url   = @AvatarUrl,
                is_active    = @IsActive,
                last_login_at = @LastLoginAt
            WHERE id = @Id;";

        await conn.ExecuteAsync(sql, user);
    }
}