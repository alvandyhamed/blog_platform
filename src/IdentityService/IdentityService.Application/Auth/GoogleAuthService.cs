using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IdentityService.Application.Auth;

// -------------------------
// Options + DTOs
// -------------------------
public sealed class GoogleOAuthOptions
{
    public string ClientId { get; set; } = null!;
    public string ClientSecret { get; set; } = null!;
    public string TokenEndpoint { get; set; } = "https://oauth2.googleapis.com/token";
    public string UserInfoEndpoint { get; set; } = "https://www.googleapis.com/oauth2/v2/userinfo";
}

public sealed class GoogleTokenResponse
{
    [JsonPropertyName("access_token")] public string AccessToken { get; set; } = null!;
    [JsonPropertyName("id_token")] public string IdToken { get; set; } = null!;
    [JsonPropertyName("token_type")] public string? TokenType { get; set; }
    [JsonPropertyName("expires_in")] public int ExpiresIn { get; set; }
}

public sealed class GoogleUserInfo
{
    [JsonPropertyName("email")] public string Email { get; set; } = null!;
    [JsonPropertyName("name")] public string Name { get; set; } = null!;
    [JsonPropertyName("picture")] public string Picture { get; set; } = null!;
    [JsonPropertyName("Sub")] public string Sub { get; set; } = null!;
}

public sealed class GoogleAuthResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Token { get; set; }
    public User? User { get; set; }
    public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();
}

// -------------------------
// Interface ها
// -------------------------
public interface IGoogleOAuthService
{
    Task<GoogleUserInfo?> GetUserInfoFromAuthCodeAsync(string code, string redirectUri, CancellationToken ct);
}

public interface IGoogleAuthService
{
    Task<GoogleAuthResult> HandleCallbackAsync(string code, string redirectUri, CancellationToken cancellationToken);
}

// -------------------------
// ارتباط مستقیم با Google
// -------------------------
public sealed class GoogleOAuthService : IGoogleOAuthService
{
    private readonly HttpClient _client;
    private readonly GoogleOAuthOptions _options;

    public GoogleOAuthService(HttpClient client, IOptions<GoogleOAuthOptions> options)
    {
        _client = client;
        _options = options.Value;
    }

    public async Task<GoogleUserInfo?> GetUserInfoFromAuthCodeAsync(string code, string redirectUri, CancellationToken ct)
    {
        var form = new Dictionary<string, string>
        {
            ["code"] = code,
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["redirect_uri"] = redirectUri,
            ["grant_type"] = "authorization_code"
        };

        var tokenRes = await _client.PostAsync(_options.TokenEndpoint, new FormUrlEncodedContent(form), ct);
        if (!tokenRes.IsSuccessStatusCode)
        {
            var body = await tokenRes.Content.ReadAsStringAsync(ct);
            throw new Exception($"Google token exchange failed: {(int)tokenRes.StatusCode} {tokenRes.ReasonPhrase} |{body}");
        }

        var tokenData = await tokenRes.Content.ReadFromJsonAsync<GoogleTokenResponse>(cancellationToken: ct);
        if (tokenData?.AccessToken is null)
            return null;

        var userReq = new HttpRequestMessage(HttpMethod.Get, _options.UserInfoEndpoint);
        userReq.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenData.AccessToken);

        var userRes = await _client.SendAsync(userReq, ct);
        if (!userRes.IsSuccessStatusCode)
            return null;
        var user = await userRes.Content.ReadFromJsonAsync<GoogleUserInfo>(cancellationToken: ct);
        if (user is null || string.IsNullOrWhiteSpace(user.Sub) || string.IsNullOrWhiteSpace(user.Email))
            return null;

        return user;
    }
}

// -------------------------
// سرویس نهایی: ساخت یوزر + نقش + JWT
// -------------------------
public sealed class GoogleAuthService : IGoogleAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserRoleRepository _roleRepository;
    private readonly IGoogleOAuthService _oauth;
    private readonly IJwtTokenGenerator _jwt;

    public GoogleAuthService(
        IUserRepository userRepository,
        IUserRoleRepository roleRepository,
        IGoogleOAuthService oauth,
        IJwtTokenGenerator jwt)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _oauth = oauth;
        _jwt = jwt;
    }

    public async Task<GoogleAuthResult> HandleCallbackAsync(string code, string redirectUri, CancellationToken cancellationToken)
    {
        // 1) دریافت اطلاعات از گوگل
        var googleUser = await _oauth.GetUserInfoFromAuthCodeAsync(code, redirectUri, cancellationToken);
        if (googleUser is null)
            return new() { Success = false, ErrorMessage = "Google login failed" };

        var googleId = googleUser.Sub!;
        var email = googleUser.Email!;
        var name = googleUser.Name ?? "User";
        var picture = googleUser.Picture;

        // 2) پیدا کردن یا ساخت کاربر

        var user = await _userRepository.GetByGoogleIdAsync(googleId);
        if (user is null)
            user = await _userRepository.GetByEmailAsync(email);

        if (user is null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                GoogleId = googleUser.Sub,
                Email = googleUser.Email,
                DisplayName = googleUser.Name,
                AvatarUrl = googleUser.Picture,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);
            await _roleRepository.AddRoleToUserAsync(user.Id, "Author");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(user.GoogleId))
            {
                user.GoogleId = googleId;
                await _userRepository.UpdateAsync(user);
            }

        }

        // 3) گرفتن نقش‌های کاربر
        var roles = (await _roleRepository.GetRolesForUserAsync(user.Id))
                        .Select(r => r.Name)
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .ToList();
        if (roles.Count == 0)
            roles.Add("Author");

        // 4) ساخت JWT
        var token = _jwt.GenerateToken(user, roles);

        return new GoogleAuthResult
        {
            Success = true,
            Token = token,
            User = user,
            Roles = roles
        };
    }
}