using IdentityService.Domain.Entities;

namespace IdentityService.Application.Interfaces;

public interface IUserRoleRepository
{
    Task<IEnumerable<Role>> GetRolesForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task AssignRoleAsync(
        UserRole userRole,
        CancellationToken cancellationToken = default);

    // 👇 این متد جدید دقیقا برای راحتی کار Google Register
    Task AddRoleToUserAsync(
        Guid userId,
        string roleName,
        CancellationToken cancellationToken = default);
}