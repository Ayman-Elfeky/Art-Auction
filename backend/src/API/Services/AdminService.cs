using ArtAuction.Application.Features.Admin.Commands.ApproveArtistAccount;
using ArtAuction.Application.Features.Admin.Commands.ApproveArtwork;
using ArtAuction.Application.Features.Admin.Commands.RejectArtistAccount;
using ArtAuction.Application.Features.Admin.Commands.RejectArtwork;
using ArtAuction.Application.Features.Admin.Queries.GetPendingArtists;
using ArtAuction.Application.Features.Admin.Queries.GetPendingArtworks;
using ArtAuction.Domain.Enums;
using MediatR;
using VeldGenerated.Models;
using VeldGenerated.Services;

namespace Api.Services;

public class AdminService : IAdminService
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserContext _currentUserContext;

    public AdminService(IMediator mediator, ICurrentUserContext currentUserContext)
    {
        _mediator = mediator;
        _currentUserContext = currentUserContext;
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
        if (!_currentUserContext.IsAuthenticated() || !_currentUserContext.IsInRole(ArtAuction.Domain.Enums.UserRole.Admin))
        {
            throw new InvalidOperationException("Only admin users can access this endpoint.");
        }
    }
}
