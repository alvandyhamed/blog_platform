using IdentityService.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace IdentityService.Infrastructure.Auth;

public class GoogleOAuthService : IGoogleOAuthService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    public GoogleOAuthService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;

    }
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
    // public async Task<GoogleUserInfo> GetUserInfoFromAuthCodeAsync(string code,string redirectUri)
    // {
    //     // var clientId     = _config["Google:ClientId"];
    //     // var clientSecret = _config["Google:ClientSecret"];

    //       var user = new GoogleUserInfo
    //     {
    //         Sub = "dummy-google-id-" + code,
    //         Email = $"user_{code}@example.com",
    //         Name = "Dummy User",
    //         Picture = null
    //     };

    //     return Task.FromResult(user);
    // }

}