using Api.Services;
using ArtAuction.Domain.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/bids")]
public class BidsController : ControllerBase
{
    private readonly IBidService _service;

    public BidsController(IBidService service) { _service = service; }

    [HttpPost]
    [Authorize(Roles = "Buyer")]
    [Authorize(Policy = Permissions.BidsPlace)]
    public async Task<IActionResult> CreateBid([FromBody] Api.Models.Bid input)
    {
        try { var result = await _service.CreateBid(input); return StatusCode(201, result); }
        catch (Exception e) { return StatusCode(500, new { error = e.Message }); }
    }

    [HttpGet("artworks/{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBids([FromRoute] string Id)
    {
        try { var result = await _service.GetBids(Id); return Ok(result); }
        catch (Exception e) { return StatusCode(500, new { error = e.Message }); }
    }
}
