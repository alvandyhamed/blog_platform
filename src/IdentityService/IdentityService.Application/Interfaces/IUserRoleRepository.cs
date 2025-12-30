using IdentityService.Domain.Entities;

namespace IdentityService.Application.Interfaces;

public interface IUserRoleRepository
{
    Task<IEnumerable<Role>> GetRolesForUserAsync(Guid userId);
    Task AssignRoleAsync(UserRole userRole);
}