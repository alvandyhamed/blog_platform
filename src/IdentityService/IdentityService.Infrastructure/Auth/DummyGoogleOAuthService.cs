using System.Threading;
using System.Threading.Tasks;
using IdentityService.Application.Auth;

namespace IdentityService.Infrastructure.Auth;

public class DummyGoogleOAuthService : IGoogleOAuthService
{
    public Task<GoogleUserInfo> GetUserInfoFromAuthCodeAsync(
        string code,
        string redirectUri,
        CancellationToken ct = default)
    {
        // اینجا به‌جای درخواست واقعی به گوگل، یه یوزر فیک می‌سازیم
        var user = new GoogleUserInfo
        {
            Email = $"user_{code}@example.com",
            Name = "Dummy User",
            Picture = null,
            Sub = $"dummy-google-id-{code}"
        };

        return Task.FromResult(user);
    }
}