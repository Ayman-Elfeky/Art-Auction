using Api.Services;
using ArtAuction.Domain.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/watchlist")]
[Authorize(Roles = "Buyer")]
public sealed class WatchlistController : ControllerBase
{
    private readonly IWatchlistService _watchlistService;

    public WatchlistController(IWatchlistService watchlistService)
    {
        _watchlistService = watchlistService;
    }

    [HttpGet]
    [Authorize(Policy = Permissions.WatchlistManage)]
    public async Task<IActionResult> GetMyWatchlist()
    {
        var result = await _watchlistService.GetMyWatchlist();
        return Ok(result);
    }

    [HttpPost("{artworkId}")]
    [Authorize(Policy = Permissions.WatchlistManage)]
    public async Task<IActionResult> AddToWatchlist([FromRoute] string artworkId)
    {
        var message = await _watchlistService.AddToWatchlist(artworkId);
        return Ok(new { message });
    }

    [HttpDelete("{artworkId}")]
    [Authorize(Policy = Permissions.WatchlistManage)]
    public async Task<IActionResult> RemoveFromWatchlist([FromRoute] string artworkId)
    {
        var result = await _watchlistService.RemoveFromWatchlist(artworkId);
        return Ok(result);
    }
}
