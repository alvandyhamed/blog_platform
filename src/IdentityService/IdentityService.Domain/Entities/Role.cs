namespace IdentityService.Domain.Entities;

public class Role
{
    public short Id { get; set; }                 // 1 = Admin, 2 = Author
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<UserRole> UserRoles { get; set; } = new();

    public Role()
    {
    }

    public Role(string name, string? description = null)
    {
        Name = name;
        Description = description;
        CreatedAt = DateTime.UtcNow;
    }

    public static class Names
    {
        public const string Admin = "Admin";
        public const string Author = "Author";
    }
}