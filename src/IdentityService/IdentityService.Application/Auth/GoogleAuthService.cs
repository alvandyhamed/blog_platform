using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;

namespace IdentityService.Application.Auth;

public class GoogleAuthResult
{
    public string Token { get; set; } = null!;
    public User User { get; set; } = null!;
    public IEnumerable<string> Roles { get; set; } = Enumerable.Empty<string>();
}

public class GoogleAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IGoogleOAuthService _googleOAuthService;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public GoogleAuthService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IUserRoleRepository userRoleRepository,
        IGoogleOAuthService googleOAuthService,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _userRoleRepository = userRoleRepository;
        _googleOAuthService = googleOAuthService;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<GoogleAuthResult> HandleCallbackAsync(string code, string redirectUri)
    {
        // 1) گرفتن اطلاعات کاربر از گوگل
        var googleUser = await _googleOAuthService.GetUserInfoFromAuthCodeAsync(code, redirectUri);

        // 2) پیدا کردن کاربر یا ساختن جدید
        var user = await _userRepository.GetByGoogleIdAsync(googleUser.Sub);
        if (user is null)
        {
            user = new User(
                googleUser.Sub,
                googleUser.Email,
                googleUser.Name,
                googleUser.Picture
            );

            await _userRepository.AddAsync(user);

            // نقش پیش‌فرض: Author
            var authorRole = await _roleRepository.GetByNameAsync(Role.Names.Author)
                            ?? throw new InvalidOperationException("Author role not seeded");

            var userRole = new UserRole(user.Id, authorRole.Id);
            await _userRoleRepository.AssignRoleAsync(userRole);
        }
        else
        {
            user.DisplayName = googleUser.Name ?? user.DisplayName;
            user.AvatarUrl = googleUser.Picture ?? user.AvatarUrl;
            user.LastLoginAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
        }

        // 3) گرفتن نقش‌های کاربر
        var roles = (await _userRoleRepository.GetRolesForUserAsync(user.Id))
                    .Select(r => r.Name)
                    .ToList();

        // 4) ساخت JWT
        var token = _jwtTokenGenerator.GenerateToken(user, roles);

        return new GoogleAuthResult
        {
            Token = token,
            User = user,
            Roles = roles
        };
    }
}