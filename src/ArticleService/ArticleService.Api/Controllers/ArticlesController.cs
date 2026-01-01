using System.Security.Claims;
using ArticleService.Application.Dtos;
using ArticleService.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using IdentityService.Grpc;



namespace ArticleService.Api.Controllers;






[ApiController]
[Route("api/[controller]")]
public class ArticlesController : ControllerBase
{
    private readonly IArticleService _articleService;

    private bool TryGetUserId(out Guid userId)
    {
        userId = Guid.Empty;

        var userIdClaim =
            User.FindFirst(JwtRegisteredClaimNames.Sub) ??
            User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
            return false;

        return Guid.TryParse(userIdClaim.Value, out userId);
    }

    public ArticlesController(IArticleService articleService)
    {
        _articleService = articleService;
    }




    // ساخت مقاله جدید – فقط Author و Admin
    [HttpPost]
    [Authorize(Roles = "Author,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateArticleRequest request, CancellationToken cancellationToken)
    {
        // گرفتن userId از JWT (claim: sub)
        var userIdClaim =
            User.FindFirst(JwtRegisteredClaimNames.Sub) ??
            User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var authorId))
        {
            return Unauthorized("Invalid user id in token.");
        }

        var id = await _articleService.CreateArticleAsync(request, authorId, cancellationToken);

        // لینک به GET /api/Articles/{id}
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    // برای تست: گرفتن مقاله بر اساس id – همه می‌توانند ببینند
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var article = await _articleService.GetByIdAsync(id, cancellationToken);
        if (article is null)
            return NotFound();

        return Ok(article);
    }


    // لیست مقالات پابلیش شده با سرچ و صفحه‌بندی
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<ArticleDto>>> GetPublished(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery(Name = "q")] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _articleService.GetPublishedAsync(
            page,
            pageSize,
            search,
            cancellationToken);

        return Ok(result);
    }

    // // لیست مقالات منتشرشده – برای کاربرهای عادی
    // [HttpGet]
    // [AllowAnonymous]
    // public async Task<IActionResult> GetPublished(
    //     [FromQuery] int page = 1,
    //     [FromQuery] int pageSize = 10,
    //     CancellationToken cancellationToken = default)
    // {
    //     if (page <= 0) page = 1;
    //     if (pageSize <= 0 || pageSize > 50) pageSize = 10;

    //     var articles = await _articleService.GetPublishedAsync(page, pageSize, null, cancellationToken);
    //     return Ok(articles);
    // }
    // تأیید (Publish) مقاله – فقط Admin
    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        var success = await _articleService.ApproveAsync(id, cancellationToken);
        if (!success)
            return NotFound();

        return NoContent();
    }

    // فقط برای تست gRPC: اطلاعات نویسنده‌ی فعلی را از IdentityService می‌گیرد
    [HttpGet("me-from-identity")]
    [Authorize] // هر یوزر لاگین شده
    public async Task<IActionResult> GetMeFromIdentity(
        [FromServices] UserService.UserServiceClient identityClient)
    {
        var userIdClaim =
            User.FindFirst(JwtRegisteredClaimNames.Sub) ??
            User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim is null)
            return Unauthorized("No user id in token");

        var reply = await identityClient.GetUserByIdAsync(
            new GetUserByIdRequest { Id = userIdClaim.Value });

        return Ok(reply);
    }

    // POST /api/Articles/{id}/like
    [HttpPost("{id:guid}/like")]
    [Authorize]
    public async Task<IActionResult> Like(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized("Invalid user id in token.");

        var result = await _articleService.ToggleReactionAsync(id, userId, true, cancellationToken);
        return Ok(result); // { likes, dislikes }
    }
    // POST /api/Articles/{id}/dislike
    [HttpPost("{id:guid}/dislike")]
    [Authorize]
    public async Task<IActionResult> Dislike(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized("Invalid user id in token.");

        var result = await _articleService.ToggleReactionAsync(id, userId, false, cancellationToken);
        return Ok(result);
    }
    public class AddCommentRequest
    {
        public string Content { get; set; } = default!;
    }

    // POST /api/Articles/{id}/comments
    [HttpPost("{id:guid}/comments")]
    [Authorize]
    public async Task<IActionResult> AddComment(
        Guid id,
        [FromBody] AddCommentRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
            return BadRequest("Content is required.");

        if (!TryGetUserId(out var userId))
            return Unauthorized("Invalid user id in token.");

        var commentId = await _articleService.AddCommentAsync(
            id, userId, request.Content, cancellationToken);

        return CreatedAtAction(
            nameof(GetComments),
            new { id },
            new { id = commentId });
    }
    // GET /api/Articles/{id}/comments
    [HttpGet("{id:guid}/comments")]
    [AllowAnonymous] // اگر می‌خواهی فقط لاگین‌ها ببینند، این رو بردار
    public async Task<IActionResult> GetComments(Guid id, CancellationToken cancellationToken)
    {
        var comments = await _articleService.GetCommentsAsync(id, cancellationToken);
        return Ok(comments);
    }

}