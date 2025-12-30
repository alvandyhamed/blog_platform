using System.Data;
using Dapper;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Data;

namespace IdentityService.Infrastructure.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public RoleRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Role?> GetByIdAsync(short id)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT id, name, description, created_at AS CreatedAt
            FROM roles
            WHERE id = @id;";

        return await conn.QuerySingleOrDefaultAsync<Role>(sql, new { id });
    }

    public async Task<Role?> GetByNameAsync(string name)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT id, name, description, created_at AS CreatedAt
            FROM roles
            WHERE name = @name;";

        return await conn.QuerySingleOrDefaultAsync<Role>(sql, new { name });
    }

    public async Task<IEnumerable<Role>> GetAllAsync()
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT id, name, description, created_at AS CreatedAt
            FROM roles
            ORDER BY id;";

        return await conn.QueryAsync<Role>(sql);
    }
}