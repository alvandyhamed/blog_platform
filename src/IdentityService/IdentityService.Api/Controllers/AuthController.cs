
using System.Security.Claims;
using IdentityService.Api.Models;
using IdentityService.Application.Auth;
using IdentityService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly GoogleAuthService _googleAuthService;
    private readonly IConfiguration _configuration;
    private readonly IUserRoleRepository _roleRepository;
    private readonly IUserRepository _userRepository;


    // public AuthController(GoogleAuthService googleAuthService, IConfiguration configuration)
    // {
    //     _googleAuthService = googleAuthService;
    //     _configuration = configuration;
    // }
    public AuthController(
    GoogleAuthService googleAuthService,
    IUserRoleRepository roleRepository,
    IUserRepository userRepository)
    {
        _googleAuthService = googleAuthService;
        _roleRepository = roleRepository;
        _userRepository = userRepository;
    }

    // فقط برای تست معماری / DB / JWT
    // فرض می‌کنیم ?code=123 رو از فرانت گرفتیم
    [HttpGet("google/callback")]
    public async Task<IActionResult> GoogleCallback([FromQuery] string code)
    {
        var redirectUri = "http://localhost:3000/auth/callback";

        var result = await _googleAuthService.HandleCallbackAsync(code, redirectUri, HttpContext.RequestAborted);
        if (!result.Success || result.User is null)
            return Unauthorized(result.ErrorMessage ?? "Google auth failed.");

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
            token = googleResult.Token,
            user = new
            {
                email = g.Email,
                name = g.DisplayName,
                picture = g.AvatarUrl,
                provider = "Google"
            }
        });
    }
    // ===============================
    // GET /api/Auth/user
    // ===============================
    [HttpGet("user")]
    [Authorize]
    public async Task<ActionResult<UserProfileResponse>> GetCurrentUser(
        CancellationToken cancellationToken)
    {
        // 1) گرفتن userId از JWT
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                         ?? User.FindFirst("sub");

        if (userIdClaim is null)
            return Unauthorized();

        if (!Guid.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized();

        // 2) گرفتن یوزر از دیتابیس
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return NotFound();

        // 3) گرفتن رول‌ها
        var roles = await _roleRepository.GetRolesForUserAsync(userId);
        var roleName = roles.FirstOrDefault()?.Name ?? "user";

        // 4) پاسخ
        return Ok(new UserProfileResponse
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.DisplayName ?? user.Email.Split('@')[0],
            Display_Name = user.DisplayName ?? "",
            Avatar_Url = user.AvatarUrl,
            Role = roleName
        });
    }


}