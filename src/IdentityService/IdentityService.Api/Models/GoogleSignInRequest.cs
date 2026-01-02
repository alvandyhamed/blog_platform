namespace IdentityService.Api.Models;

public class GoogleSignInRequest
{
    public string Code { get; set; } = default!;
    public string RedirectUri { get; set; } = default!;
}