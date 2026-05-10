using Api.Models;
using Api.Services;
using ArtAuction.Domain.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/artworks")]
[AllowAnonymous]
public class ArtworksController : ControllerBase
{
    private readonly IArtworksService _service;

    public ArtworksController(IArtworksService service) { _service = service; }

    [HttpGet]
    public async Task<IActionResult> GetArtworks([FromQuery] Dictionary<string, string> query)
    {
        try { var result = await _service.GetArtworks(query); return Ok(result); }
        catch (Exception e) { return StatusCode(500, new { error = e.Message }); }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetArtworkById([FromRoute] string Id)
    {
        try { var result = await _service.GetArtworkById(Id); return Ok(result); }
        catch (Exception e) { return StatusCode(500, new { error = e.Message }); }
    }

    [HttpGet("artist/{artistId}")]
    public async Task<IActionResult> GetArtworksByArtist([FromRoute] string ArtistId, [FromQuery] Dictionary<string, string> query)
    {
        try { var result = await _service.GetArtworksByArtist(ArtistId, query); return Ok(result); }
        catch (Exception e) { return StatusCode(500, new { error = e.Message }); }
    }

    [HttpPost]
    [Authorize(Roles = "Artist")]
    [Authorize(Policy = Permissions.ArtworksCreate)]
    public async Task<IActionResult> CreateArtwork([FromBody] CreateArtworkInput input)
    {
        try { var result = await _service.CreateArtwork(input); return StatusCode(201, result); }
        catch (Exception e) { return StatusCode(500, new { error = e.Message }); }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Artist")]
    [Authorize(Policy = Permissions.ArtworksUpdate)]
    public async Task<IActionResult> UpdateArtwork([FromRoute] string Id, [FromBody] CreateArtworkInput input)
    {
        try { var result = await _service.UpdateArtwork(Id, input); return Ok(result); }
        catch (Exception e) { return StatusCode(500, new { error = e.Message }); }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Artist")]
    [Authorize(Policy = Permissions.ArtworksDelete)]
    public async Task<IActionResult> DeleteArtwork([FromRoute] string Id)
    {
        try { var result = await _service.DeleteArtwork(Id); return Ok(result); }
        catch (Exception e) { return StatusCode(500, new { error = e.Message }); }
    }

    [HttpPut("{id}/extend")]
    [Authorize(Roles = "Artist")]
    [Authorize(Policy = Permissions.ArtworksExtendAuction)]
    public async Task<IActionResult> ExtendAuctionTime([FromRoute] string Id, [FromBody] ExtendAuctionTimeInput input)
    {
        try { var result = await _service.ExtendAuctionTime(Id, input); return Ok(result); }
        catch (Exception e) { return StatusCode(500, new { error = e.Message }); }
    }
}
