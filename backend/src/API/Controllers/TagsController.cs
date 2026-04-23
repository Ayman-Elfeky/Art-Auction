using ArtAuction.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/tags")]
[AllowAnonymous]
public sealed class TagsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TagsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> ListTags()
    {
        var tags = await _context.ArtworkTags
            .Where(t => !string.IsNullOrWhiteSpace(t.Tag))
            .Select(t => t.Tag.Trim())
            .Distinct()
            .OrderBy(t => t)
            .ToListAsync();

        return Ok(tags);
    }
}
