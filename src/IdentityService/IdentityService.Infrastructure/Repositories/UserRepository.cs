
using Dapper;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;

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

    public async Task<IReadOnlyList<string>> GetUserRolesAsync(
    Guid userId,
    CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT r.name
FROM user_roles ur
JOIN roles r ON ur.role_id = r.id
WHERE ur.user_id = @UserId;
";

        using var conn = _connectionFactory.CreateConnection();
        conn.Open();

        var result = await conn.QueryAsync<string>(
            new CommandDefinition(
                sql,
                new { UserId = userId },
                cancellationToken: cancellationToken
            ));

        return result.AsList();
    }
}