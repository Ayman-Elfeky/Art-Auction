using ArtAuction.Application.Common.Interfaces;
using ArtAuction.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArtAuction.Application.Features.Admin.Commands.ApproveArtistAccount
{
    public class ApproveArtistAccountCommandHandler
        : IRequestHandler<ApproveArtistAccountCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public ApproveArtistAccountCommandHandler(
            IApplicationDbContext context,
            INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<bool> Handle(
            ApproveArtistAccountCommand request,
            CancellationToken cancellationToken)
        {
            var artist = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == request.ArtistId && u.Role == UserRole.Artist, cancellationToken);

            if (artist == null) return false;

            artist.IsApproved = true;
            artist.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            await _notificationService.SendNotificationAsync(
                userId: artist.Id,
                title: "Account Approved",
                message: "Congratulations! Your artist account has been approved.",
                type: NotificationType.AccountApproved,
                relatedArtworkId: null
            );

            return true;
        }
    }
}