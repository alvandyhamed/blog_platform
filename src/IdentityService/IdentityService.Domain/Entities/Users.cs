namespace IdentityService.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string GoogleId { get; set; } = null;
    public string Email { get; set; } = null;
    public string? DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public List<UserRole> UserRoles { get; set; } = new();

    public User()
    {
        // برای Dapper و ORM لازم هست
    }
    public User(string googleId, string email, string? displayName = null, string? avatarUrl = null)
    {
        Id = Guid.NewGuid();
        GoogleId = googleId;
        Email = email.Trim().ToLowerInvariant();
        DisplayName = displayName;
        AvatarUrl = avatarUrl;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

}
