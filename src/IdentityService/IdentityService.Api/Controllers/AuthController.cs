
using IdentityService.Api.Models;
using IdentityService.Application.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly GoogleAuthService _googleAuthService;
    private readonly IConfiguration _configuration;

    public AuthController(GoogleAuthService googleAuthService, IConfiguration configuration)
    {
        _googleAuthService = googleAuthService;
        _configuration = configuration;
    }

    // فقط برای تست معماری / DB / JWT
    // فرض می‌کنیم ?code=123 رو از فرانت گرفتیم
    [HttpGet("google/callback")]
    public async Task<IActionResult> GoogleCallback([FromQuery] string code)
    {
        var redirectUri = "http://localhost:5089/api/auth/google/callback";

        var result = await _googleAuthService.HandleCallbackAsync(code, redirectUri, HttpContext.RequestAborted);

        return Ok(new
        {
            token = result.Token,
            user = new
            {
                id = result.User.Id,
                email = result.User.Email,
                name = result.User.DisplayName,
                avatar = result.User.AvatarUrl,
                roles = result.Roles
            }
        });
    }
    // POST: /api/auth/google
    [HttpPost("google")]
    [AllowAnonymous]
    public async Task<IActionResult> SignInWithGoogle(
        [FromBody] GoogleSignInRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
            return BadRequest("code is required");

        var googleResult = await _googleAuthService.HandleCallbackAsync(
            request.Code,
            request.RedirectUri,
            cancellationToken);

        if (!googleResult.Success || googleResult.User is null)
            return Unauthorized(googleResult.ErrorMessage ?? "Google auth failed.");

        var g = googleResult.User;

        // -------------------------------
        // TODO: اینجا می‌تونی لاجیک واقعی‌ت رو بذاری:
        //  - اگر کاربر با این Email وجود داشت، بیار
        //  - اگر نبود، یوزر جدید بساز
        //  - بعد با JwtTokenGenerator خودت براش JWT بساز
        //
        // فعلاً برای این‌که سیستم کار کنه، یه توکن دامی برمی‌گردونیم.
        // -------------------------------
        var dummyJwt = $"google-jwt-for-{g.Email}";

        return Ok(new
        {
            token = dummyJwt,
            user = new
            {
                email = g.Email,
                name = g.DisplayName,
                picture = g.AvatarUrl,
                provider = "Google"
            }
        });
    }
}