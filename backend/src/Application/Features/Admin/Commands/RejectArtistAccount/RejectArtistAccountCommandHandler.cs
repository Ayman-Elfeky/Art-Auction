using ArtAuction.Application.Common.Interfaces;
using ArtAuction.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArtAuction.Application.Features.Admin.Commands.RejectArtistAccount
{
    public class RejectArtistAccountCommandHandler
        : IRequestHandler<RejectArtistAccountCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public RejectArtistAccountCommandHandler(
            IApplicationDbContext context,
            INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<bool> Handle(
            RejectArtistAccountCommand request,
            CancellationToken cancellationToken)
        {
            var artist = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == request.ArtistId && u.Role == UserRole.Artist, cancellationToken);

            if (artist == null) return false;
            if (!artist.IsActive) return false;
            if (artist.IsApproved) return false;

            artist.IsActive = false;
            artist.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            await _notificationService.SendNotificationAsync(
                userId: artist.Id,
                title: "Account Rejected",
                message: "Your artist account registration has been rejected.",
                type: NotificationType.AccountRejected,
                relatedArtworkId: null
            );

            return true;
        }
    }
}