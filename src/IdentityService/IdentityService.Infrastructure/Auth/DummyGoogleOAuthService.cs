using IdentityService.Application.Interfaces;

namespace IdentityService.Infrastructure.Auth;

// فقط برای تست، بعداً با سرویس واقعی گوگل عوضش می‌کنیم
public class DummyGoogleOAuthService : IGoogleOAuthService
{
    public Task<GoogleUserInfo> GetUserInfoFromAuthCodeAsync(string code, string redirectUri)
    {
        // در محیط واقعی به Google token endpoint می‌زنی؛
        // فعلاً یه یوزر فیک می‌سازیم برای تست.
        var user = new GoogleUserInfo
        {
            Sub = "dummy-google-id-" + code,
            Email = $"user_{code}@example.com",
            Name = "Dummy User",
            Picture = null
        };

        return Task.FromResult(user);
    }
}