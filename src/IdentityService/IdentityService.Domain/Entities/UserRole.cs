namespace IdentityService.Domain.Entities;

public class UserRole
{
    public Guid UserId { get; set; }
    public short RoleId { get; set; }
    public DateTime AssignedAt { get; set; }

    // Navigation اختیاری
    public User? User { get; set; }
    public Role? Role { get; set; }

    public UserRole()
    {
    }

    public UserRole(Guid userId, short roleId)
    {
        UserId = userId;
        RoleId = roleId;
        AssignedAt = DateTime.UtcNow;
    }
}