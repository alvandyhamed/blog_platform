using IdentityService.Domain.Entities;

namespace IdentityService.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<User?> GetByGoogleIdAsync(
        string googleId,
        CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetUserRolesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        User user,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        User user,
        CancellationToken cancellationToken = default);
}