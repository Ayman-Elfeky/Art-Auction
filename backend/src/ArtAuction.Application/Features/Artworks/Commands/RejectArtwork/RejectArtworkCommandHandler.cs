using ArtAuction.Application.Common.Exceptions;
using ArtAuction.Application.Common.Interfaces;
using ArtAuction.Application.Common.Models;
using ArtAuction.Domain.Entities;
using ArtAuction.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArtAuction.Application.Features.Artworks.Commands.RejectArtwork;

public class RejectArtworkCommandHandler
    : IRequestHandler<RejectArtworkCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public RejectArtworkCommandHandler(
        IApplicationDbContext context,
        INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<Result<bool>> Handle(
        RejectArtworkCommand request,
        CancellationToken cancellationToken)
    {
        var artwork = await _context.Artworks
            .FirstOrDefaultAsync(a => a.Id == request.ArtworkId, cancellationToken);

        if (artwork == null)
            throw new NotFoundException(nameof(Artwork), request.ArtworkId);

        artwork.Status = ArtworkStatus.Rejected;
        artwork.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // Notify artist
        await _notificationService.SendNotificationAsync(
            userId: artwork.ArtistId,
            title: "Artwork Rejected",
            message: $"Your artwork '{artwork.Title}' was rejected. Reason: {request.Reason}",
            type: NotificationType.ArtworkRejected,
            relatedArtworkId: artwork.Id);

        return Result<bool>.Success(true);
    }
}