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

    // 🔹 گرفتن لیست نقش‌های کاربر
    public async Task<IEnumerable<Role>> GetRolesForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT r.id, r.name, r.description, r.created_at AS CreatedAt
            FROM user_roles ur
            JOIN roles r ON ur.role_id = r.id
            WHERE ur.user_id = @UserId;";

        using var conn = _connectionFactory.CreateConnection();
        return await conn.QueryAsync<Role>(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));
    }

    // 🔹 گرفتن نام نقش‌های کاربر به صورت string
    public async Task<IReadOnlyList<string>> GetUserRolesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT r.name
            FROM user_roles ur
            JOIN roles r ON ur.role_id = r.id
            WHERE ur.user_id = @UserId;";

        using var conn = _connectionFactory.CreateConnection();
        var result = await conn.QueryAsync<string>(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));

        return result.AsList();
    }

    // 🔹 اختصاص مستقیم UserRole با ساخت شیء
    public async Task AssignRoleAsync(
        UserRole userRole,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO user_roles (user_id, role_id, assigned_at)
            VALUES (@UserId, @RoleId, @AssignedAt)
            ON CONFLICT (user_id, role_id) DO NOTHING;";

        using var conn = _connectionFactory.CreateConnection();
        await conn.ExecuteAsync(
            new CommandDefinition(sql, userRole, cancellationToken: cancellationToken));
    }

    // 🔥 دادن نقش به کاربر فقط با اسم نقش (راحت ترین حالت)
    public async Task AddRoleToUserAsync(
        Guid userId,
        string roleName,
        CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();

        // ۱) پیدا کردن role_id
        const string getRoleSql = @"SELECT id FROM roles WHERE name = @RoleName;";
        var roleId = await conn.ExecuteScalarAsync<Guid?>(
            new CommandDefinition(getRoleSql, new { RoleName = roleName }, cancellationToken: cancellationToken)
        );

        if (roleId is null)
            throw new Exception($"⚠️ Role '{roleName}' not found in roles table!");

        // ۲) افزودن در user_roles
        const string insertSql = @"
            INSERT INTO user_roles (user_id, role_id, assigned_at)
            VALUES (@UserId, @RoleId, NOW())
            ON CONFLICT (user_id, role_id) DO NOTHING;";

        await conn.ExecuteAsync(
            new CommandDefinition(insertSql, new { UserId = userId, RoleId = roleId }, cancellationToken: cancellationToken));
    }
}