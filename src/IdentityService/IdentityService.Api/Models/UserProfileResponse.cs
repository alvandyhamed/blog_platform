namespace IdentityService.Api.Models;

public sealed class UserProfileResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Display_Name { get; set; } = null!;
    public string? Avatar_Url { get; set; }
    public string Role { get; set; } = "user";
}