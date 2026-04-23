using ArtAuction.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/bid-history")]
[AllowAnonymous]
public sealed class BidHistoryController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public BidHistoryController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("artworks/{artworkId}")]
    public async Task<IActionResult> GetArtworkBidHistory([FromRoute] string artworkId)
    {
        if (!Guid.TryParse(artworkId, out var parsedArtworkId))
        {
            return BadRequest(new { error = "Invalid artwork id." });
        }

        var history = await _context.Bids
            .Where(b => b.ArtworkId == parsedArtworkId)
            .Include(b => b.Buyer)
            .OrderByDescending(b => b.PlacedAt)
            .Select(b => new
            {
                bidderName = b.Buyer.Username,
                price = b.Amount,
                timestamp = b.PlacedAt
            })
            .ToListAsync();

        return Ok(history);
    }
}
