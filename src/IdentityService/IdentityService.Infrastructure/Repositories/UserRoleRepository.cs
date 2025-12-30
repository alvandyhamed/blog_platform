using System.Data;
using Dapper;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Data;

namespace IdentityService.Infrastructure.Repositories;

public class UserRoleRepository : IUserRoleRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserRoleRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Role>> GetRolesForUserAsync(Guid userId)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT r.id, r.name, r.description, r.created_at AS CreatedAt
            FROM user_roles ur
            JOIN roles r ON ur.role_id = r.id
            WHERE ur.user_id = @userId;";

        return await conn.QueryAsync<Role>(sql, new { userId });
    }

    public async Task AssignRoleAsync(UserRole userRole)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO user_roles (user_id, role_id, assigned_at)
            VALUES (@UserId, @RoleId, @AssignedAt)
            ON CONFLICT (user_id, role_id) DO NOTHING;";

        await conn.ExecuteAsync(sql, userRole);
    }
}