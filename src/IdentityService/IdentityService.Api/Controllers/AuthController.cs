using IdentityService.Application.Auth;
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

        var result = await _googleAuthService.HandleCallbackAsync(code, redirectUri);

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
}