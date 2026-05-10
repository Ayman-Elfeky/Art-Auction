using ArtAuction.Application.Common.Interfaces;
using ArtAuction.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

/// <summary>
/// Public bid history endpoint — demonstrates IRepository&lt;T&gt; usage.
/// </summary>
[ApiController]
[Route("api/bid-history")]
[AllowAnonymous]
public sealed class BidHistoryController : ControllerBase
{
    private readonly IRepository<Bid> _bidRepository;

    public BidHistoryController(IRepository<Bid> bidRepository)
    {
        _bidRepository = bidRepository;
    }

    [HttpGet("artworks/{artworkId}")]
    public async Task<IActionResult> GetArtworkBidHistory([FromRoute] string artworkId)
    {
        if (!Guid.TryParse(artworkId, out var parsedArtworkId))
        {
            return BadRequest(new { error = "Invalid artwork id." });
        }

        var history = await _bidRepository
            .Query()
            .Where(b => b.ArtworkId == parsedArtworkId)
            .Include(b => b.Buyer)
            .OrderByDescending(b => b.PlacedAt)
            .Select(b => new
            {
                bidderName = b.Buyer.Username,
                price      = b.Amount,
                timestamp  = b.PlacedAt
            })
            .ToListAsync();

        return Ok(history);
    }
}
