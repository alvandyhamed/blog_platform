using System.Security.Claims;
using ArticleService.Application.Dtos;
using ArticleService.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace ArticleService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArticlesController : ControllerBase
{
    private readonly IArticleService _articleService;

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
    // لیست مقالات منتشرشده – برای کاربرهای عادی
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublished(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0 || pageSize > 50) pageSize = 10;

        var articles = await _articleService.GetPublishedAsync(page, pageSize, cancellationToken);
        return Ok(articles);
    }
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

}