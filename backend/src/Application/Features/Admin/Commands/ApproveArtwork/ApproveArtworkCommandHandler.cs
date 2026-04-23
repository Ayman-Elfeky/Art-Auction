using ArtAuction.Application.Common.Interfaces;
using ArtAuction.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArtAuction.Application.Features.Admin.Commands.ApproveArtwork
{
    public class ApproveArtworkCommandHandler
        : IRequestHandler<ApproveArtworkCommand, bool>
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

        public async Task<bool> Handle(
            ApproveArtworkCommand request,
            CancellationToken cancellationToken)
        {
            var artwork = await _context.Artworks
                .FirstOrDefaultAsync(a => a.Id == request.ArtworkId, cancellationToken);

            if (artwork == null) return false;
            if (artwork.Status != ArtworkStatus.Pending) return false;

            artwork.Status = ArtworkStatus.Approved;
            artwork.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            await _notificationService.SendNotificationAsync(
                userId: artwork.ArtistId,
                title: "Artwork Approved",
                message: $"Your artwork '{artwork.Title}' has been approved and is now live!",
                type: NotificationType.ArtworkApproved,
                relatedArtworkId: artwork.Id
            );

            return true;
        }
    }
}