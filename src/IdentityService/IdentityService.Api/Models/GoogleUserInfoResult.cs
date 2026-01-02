namespace IdentityService.Api.Models;

public class GoogleUserInfoResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public GoogleUserInfo? User { get; init; }

    public static GoogleUserInfoResult Fail(string message) =>
        new() { Success = false, ErrorMessage = message };

    public static GoogleUserInfoResult Ok(GoogleUserInfo user) =>
        new() { Success = true, User = user };
}