using ArtAuction.Application.Features.Admin.Commands.ApproveArtistAccount;
using ArtAuction.Application.Features.Admin.Commands.ApproveArtwork;
using ArtAuction.Application.Features.Admin.Commands.RejectArtistAccount;
using ArtAuction.Application.Features.Admin.Commands.RejectArtwork;
using ArtAuction.Application.Features.Admin.Queries.GetPendingArtists;
using ArtAuction.Application.Features.Admin.Queries.GetPendingArtworks;
using MediatR;
using System.Security.Claims;
using VeldGenerated.Models;
using VeldGenerated.Services;

namespace Api.Services;

public class AdminService : IAdminService
{
    private readonly IMediator _mediator;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AdminService(IMediator mediator, IHttpContextAccessor httpContextAccessor)
    {
        _mediator = mediator;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<List<PendingArtist>> GetPendingArtists()
    {
        EnsureAdmin();

        var result = await _mediator.Send(new GetPendingArtistsQuery());
        return result
            .Select(x => new PendingArtist(x.Id, x.Username, x.Email, x.CreatedAt))
            .ToList();
    }

    public async Task<AdminActionMessage> ApproveArtist(string Id)
    {
        EnsureAdmin();

        if (!Guid.TryParse(Id, out var artistId))
        {
            throw new InvalidOperationException("Invalid artist id.");
        }

        var success = await _mediator.Send(new ApproveArtistAccountCommand(artistId));
        return new AdminActionMessage(success ? "Artist approved." : "Artist not found.");
    }

    public async Task<AdminActionMessage> RejectArtist(string Id)
    {
        EnsureAdmin();

        if (!Guid.TryParse(Id, out var artistId))
        {
            throw new InvalidOperationException("Invalid artist id.");
        }

        var success = await _mediator.Send(new RejectArtistAccountCommand(artistId));
        return new AdminActionMessage(success ? "Artist rejected." : "Artist not found.");
    }

    public async Task<List<PendingArtwork>> GetPendingArtworks()
    {
        EnsureAdmin();

        var result = await _mediator.Send(new GetPendingArtworksQuery());
        return result
            .Select(x => new PendingArtwork(
                x.Id,
                x.Title,
                x.Description,
                (double)x.InitialPrice,
                x.ArtistName,
                x.CreatedAt))
            .ToList();
    }

    public async Task<AdminActionMessage> ApproveArtwork(string Id)
    {
        EnsureAdmin();

        if (!Guid.TryParse(Id, out var artworkId))
        {
            throw new InvalidOperationException("Invalid artwork id.");
        }

        var success = await _mediator.Send(new ApproveArtworkCommand(artworkId));
        return new AdminActionMessage(success ? "Artwork approved." : "Artwork not found.");
    }

    public async Task<AdminActionMessage> RejectArtwork(string Id)
    {
        EnsureAdmin();

        if (!Guid.TryParse(Id, out var artworkId))
        {
            throw new InvalidOperationException("Invalid artwork id.");
        }

        var success = await _mediator.Send(new RejectArtworkCommand(artworkId));
        return new AdminActionMessage(success ? "Artwork rejected." : "Artwork not found.");
    }

    private void EnsureAdmin()
    {
        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("No HTTP context found.");
        if (httpContext.User?.Identity?.IsAuthenticated != true)
        {
            throw new InvalidOperationException("Invalid or missing bearer token.");
        }

        var role = httpContext.User.FindFirst(ClaimTypes.Role)?.Value
            ?? httpContext.User.FindFirst("role")?.Value;

        if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Only admin users can access this endpoint.");
        }
    }
}
