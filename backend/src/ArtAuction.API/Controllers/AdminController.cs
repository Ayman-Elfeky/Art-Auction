using ArtAuction.Application.Features.Admin.Commands.ApproveArtistAccount;
using ArtAuction.Application.Features.Admin.Commands.ApproveArtwork;
using ArtAuction.Application.Features.Admin.Commands.RejectArtistAccount;
using ArtAuction.Application.Features.Admin.Commands.RejectArtwork;
using ArtAuction.Application.Features.Admin.Queries.GetPendingArtists;
using ArtAuction.Application.Features.Admin.Queries.GetPendingArtworks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtAuction.API.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly ISender _sender;

        public AdminController(ISender sender)
        {
            _sender = sender;
        }

        // GET /api/admin/artists/pending
        [HttpGet("artists/pending")]
        public async Task<IActionResult> GetPendingArtists()
        {
            var result = await _sender.Send(new GetPendingArtistsQuery());
            return Ok(result);
        }

        // PUT /api/admin/artists/{id}/approve
        [HttpPut("artists/{id}/approve")]
        public async Task<IActionResult> ApproveArtist(Guid id)  // ✅ int → Guid
        {
            var success = await _sender.Send(new ApproveArtistAccountCommand(id));
            if (!success)
                return NotFound(new { message = $"Artist with ID {id} not found." });
            return Ok(new { message = "Artist approved successfully." });
        }

        // PUT /api/admin/artists/{id}/reject
        [HttpPut("artists/{id}/reject")]
        public async Task<IActionResult> RejectArtist(Guid id)   // ✅ int → Guid
        {
            var success = await _sender.Send(new RejectArtistAccountCommand(id));
            if (!success)
                return NotFound(new { message = $"Artist with ID {id} not found." });
            return Ok(new { message = "Artist rejected successfully." });
        }

        // GET /api/admin/artworks/pending
        [HttpGet("artworks/pending")]
        public async Task<IActionResult> GetPendingArtworks()
        {
            var result = await _sender.Send(new GetPendingArtworksQuery());
            return Ok(result);
        }

        // PUT /api/admin/artworks/{id}/approve
        [HttpPut("artworks/{id}/approve")]
        public async Task<IActionResult> ApproveArtwork(Guid id)  // ✅ int → Guid
        {
            var success = await _sender.Send(new ApproveArtworkCommand(id));
            if (!success)
                return NotFound(new { message = $"Artwork with ID {id} not found." });
            return Ok(new { message = "Artwork approved successfully." });
        }

        // PUT /api/admin/artworks/{id}/reject
        [HttpPut("artworks/{id}/reject")]
        public async Task<IActionResult> RejectArtwork(Guid id)   // ✅ int → Guid
        {
            var success = await _sender.Send(new RejectArtworkCommand(id));
            if (!success)
                return NotFound(new { message = $"Artwork with ID {id} not found." });
            return Ok(new { message = "Artwork rejected successfully." });
        }
    }
}