namespace IdentityService.Application.Interfaces;

public class GoogleUserInfo
{
    public string Sub { get; set; } = null!;     // Google unique id
    public string Email { get; set; } = null!;
    public string? Name { get; set; }
    public string? Picture { get; set; }
}

public interface IGoogleOAuthService
{
    Task<GoogleUserInfo> GetUserInfoFromAuthCodeAsync(string code, string redirectUri);
}