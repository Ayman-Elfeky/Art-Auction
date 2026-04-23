using ArtAuction.Application.Common.Interfaces;
using ArtAuction.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArtAuction.Application.Features.Admin.Commands.RejectArtwork
{
    public class RejectArtworkCommandHandler
        : IRequestHandler<RejectArtworkCommand, bool>
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

        public async Task<bool> Handle(
            RejectArtworkCommand request,
            CancellationToken cancellationToken)
        {
            var artwork = await _context.Artworks
                .FirstOrDefaultAsync(a => a.Id == request.ArtworkId, cancellationToken);

            if (artwork == null) return false;
            if (artwork.Status != ArtworkStatus.Pending) return false;

            artwork.Status = ArtworkStatus.Rejected;
            artwork.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            await _notificationService.SendNotificationAsync(
                userId: artwork.ArtistId,
                title: "Artwork Rejected",
                message: $"Your artwork '{artwork.Title}' has been rejected.",
                type: NotificationType.ArtworkRejected,
                relatedArtworkId: artwork.Id
            );

            return true;
        }
    }
}