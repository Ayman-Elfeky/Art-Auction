using ArtAuction.Application.Features.Artworks.Commands.CreateArtwork;
using ArtAuction.Application.Features.Artworks.Commands.DeleteArtwork;
using ArtAuction.Application.Features.Artworks.Commands.ExtendAuctionTime;
using ArtAuction.Application.Features.Artworks.Commands.UpdateArtwork;
using ArtAuction.Application.Features.Artworks.DTOs;
using ArtAuction.Application.Features.Artworks.Queries.GetArtworkById;
using ArtAuction.Application.Features.Artworks.Queries.GetArtworks;
using ArtAuction.Application.Features.Artworks.Queries.GetArtworksByArtist;
using ArtAuction.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ArtAuction.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArtworksController : ControllerBase
{
    private readonly IMediator _mediator;

    public ArtworksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all artworks with optional filtering and pagination
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetArtworks(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? artistName = null,
        [FromQuery] string? categoryName = null,
        [FromQuery] string? tagName = null,
        [FromQuery] string? status = null)
    {
        var filterParams = new ArtworkFilterParams
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            ArtistName = artistName,
            CategoryName = categoryName,
            TagName = tagName,
            Status = status
        };

        var result = await _mediator.Send(new GetArtworksQuery(filterParams));
        return Ok(result);
    }

    /// <summary>
    /// Get detailed information about a specific artwork
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetArtworkById(Guid id)
    {
        var result = await _mediator.Send(new GetArtworkByIdQuery(id));
        if (!result.IsSuccess)
        {
            return NotFound(result.Error);
        }
        return Ok(result.Data);
    }

    /// <summary>
    /// Get all artworks by a specific artist
    /// </summary>
    [HttpGet("artist/{artistId}")]
    public async Task<IActionResult> GetArtworksByArtist(
        Guid artistId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var paginationParams = new PaginationParams
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(new GetArtworksByArtistQuery(artistId, paginationParams));
        return Ok(result);
    }

    /// <summary>
    /// Create a new artwork (Artist only)
    /// </summary>
    [Authorize(Roles = "Artist")]
    [HttpPost]
    public async Task<IActionResult> CreateArtwork([FromBody] CreateArtworkDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Invalid user ID");
        }

        var result = await _mediator.Send(new CreateArtworkCommand(dto, userId));
        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }

        return CreatedAtAction(nameof(GetArtworkById), new { id = result.Data!.Id }, result.Data);
    }

    /// <summary>
    /// Update an artwork (Artist/Owner only)
    /// </summary>
    [Authorize(Roles = "Artist")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateArtwork(Guid id, [FromBody] CreateArtworkDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Invalid user ID");
        }

        var result = await _mediator.Send(new UpdateArtworkCommand(id, dto, userId));
        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Delete an artwork (Artist/Owner only)
    /// </summary>
    [Authorize(Roles = "Artist")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteArtwork(Guid id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Invalid user ID");
        }

        var result = await _mediator.Send(new DeleteArtworkCommand(id, userId));
        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }

        return NoContent();
    }

    /// <summary>
    /// Extend auction end time (Artist/Owner only)
    /// </summary>
    [Authorize(Roles = "Artist")]
    [HttpPut("{id}/extend")]
    public async Task<IActionResult> ExtendAuctionTime(Guid id, [FromBody] ExtendAuctionTimeDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Invalid user ID");
        }

        var result = await _mediator.Send(new ExtendAuctionTimeCommand(id, dto.NewEndTime, userId));
        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Data);
    }
}
