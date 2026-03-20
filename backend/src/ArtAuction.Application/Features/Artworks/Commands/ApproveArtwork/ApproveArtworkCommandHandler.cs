using ArtAuction.Application.Common.Exceptions;
using ArtAuction.Application.Common.Interfaces;
using ArtAuction.Application.Common.Models;
using ArtAuction.Domain.Entities;
using ArtAuction.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArtAuction.Application.Features.Artworks.Commands.ApproveArtwork;

public class ApproveArtworkCommandHandler
    : IRequestHandler<ApproveArtworkCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public ApproveArtworkCommandHandler(
        IApplicationDbContext context,
        INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<Result<bool>> Handle(
        ApproveArtworkCommand request,
        CancellationToken cancellationToken)
    {
        var artwork = await _context.Artworks
            .FirstOrDefaultAsync(a => a.Id == request.ArtworkId, cancellationToken);

        if (artwork == null)
            throw new NotFoundException(nameof(Artwork), request.ArtworkId);

        artwork.Status = ArtworkStatus.Approved;
        artwork.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // Notify artist
        await _notificationService.SendNotificationAsync(
            userId: artwork.ArtistId,
            title: "Artwork Approved",
            message: $"Your artwork '{artwork.Title}' has been approved and is now visible to buyers.",
            type: NotificationType.ArtworkApproved,
            relatedArtworkId: artwork.Id);

        return Result<bool>.Success(true);
    }
}