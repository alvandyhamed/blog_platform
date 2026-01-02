namespace IdentityService.Api.Models;

public class GoogleUserInfo
{
    public string Sub { get; set; } = default!;          // شناسه یکتای کاربر در گوگل
    public string Email { get; set; } = default!;
    public bool EmailVerified { get; set; }
    public string Name { get; set; } = default!;
    public string Picture { get; set; } = default!;
}