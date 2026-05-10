using Api.Models;
using Api.Services;
using ArtAuction.Application.Features.Admin.Commands.CreateCategory;
using ArtAuction.Application.Features.Admin.Commands.CreateTag;
using ArtAuction.Application.Features.Admin.Queries.GetAllCategories;
using ArtAuction.Application.Features.Admin.Queries.GetAllTags;
using ArtAuction.Domain.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _service;
    private readonly ISender _sender;

    public AdminController(IAdminService service, ISender sender)
    {
        _service = service;
        _sender = sender;
    }

    [HttpGet("artists/pending")]
    [Authorize(Policy = Permissions.ManageArtistAccounts)]
    public async Task<IActionResult> GetPendingArtists()
    {
        try { var result = await _service.GetPendingArtists(); return Ok(result); }
        catch (Exception e) { return StatusCode(500, new { error = e.Message }); }
    }

    [HttpPut("artists/{id}/approve")]
    [Authorize(Policy = Permissions.ManageArtistAccounts)]
    public async Task<IActionResult> ApproveArtist([FromRoute] string Id)
    {
        try { var result = await _service.ApproveArtist(Id); return Ok(result); }
        catch (Exception e) { return StatusCode(500, new { error = e.Message }); }
    }

    [HttpPut("artists/{id}/reject")]
    [Authorize(Policy = Permissions.ManageArtistAccounts)]
    public async Task<IActionResult> RejectArtist([FromRoute] string Id)
    {
        try { var result = await _service.RejectArtist(Id); return Ok(result); }
        catch (Exception e) { return StatusCode(500, new { error = e.Message }); }
    }

    [HttpGet("artworks/pending")]
    [Authorize(Policy = Permissions.ReviewArtworks)]
    public async Task<IActionResult> GetPendingArtworks()
    {
        try { var result = await _service.GetPendingArtworks(); return Ok(result); }
        catch (Exception e) { return StatusCode(500, new { error = e.Message }); }
    }

    [HttpPut("artworks/{id}/approve")]
    [Authorize(Policy = Permissions.ReviewArtworks)]
    public async Task<IActionResult> ApproveArtwork([FromRoute] string Id)
    {
        try { var result = await _service.ApproveArtwork(Id); return Ok(result); }
        catch (Exception e) { return StatusCode(500, new { error = e.Message }); }
    }

    [HttpPut("artworks/{id}/reject")]
    [Authorize(Policy = Permissions.ReviewArtworks)]
    public async Task<IActionResult> RejectArtwork([FromRoute] string Id)
    {
        try { var result = await _service.RejectArtwork(Id); return Ok(result); }
        catch (Exception e) { return StatusCode(500, new { error = e.Message }); }
    }

    // ─── CATEGORY ENDPOINTS ───────────────────────────────────────────────────

    [HttpPost("categories")]
    [Authorize(Policy = Permissions.CatalogManage)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryCommand command)
    {
        try { var result = await _sender.Send(command); return Ok(result); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    [HttpGet("categories")]
    [Authorize(Policy = Permissions.CatalogManage)]
    public async Task<IActionResult> GetAllCategories()
    {
        var result = await _sender.Send(new GetAllCategoriesQuery());
        return Ok(result);
    }

    // ─── TAG ENDPOINTS ────────────────────────────────────────────────────────

    [HttpPost("tags")]
    [Authorize(Policy = Permissions.CatalogManage)]
    public async Task<IActionResult> CreateTag([FromBody] CreateTagCommand command)
    {
        try { var result = await _sender.Send(command); return Ok(result); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    [HttpGet("tags")]
    [Authorize(Policy = Permissions.CatalogManage)]
    public async Task<IActionResult> GetAllTags()
    {
        var result = await _sender.Send(new GetAllTagsQuery());
        return Ok(result);
    }
}
