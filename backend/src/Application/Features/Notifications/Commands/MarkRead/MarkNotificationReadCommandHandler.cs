using ArtAuction.Application.Common.Interfaces;
using ArtAuction.Application.Common.Models;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ArtAuction.Application.Features.Notifications.Commands.MarkRead;

public class MarkNotificationReadCommandHandler : IRequestHandler<MarkNotificationReadCommand, Result<string>>
{
    private readonly IRepository<Domain.Entities.Notification> _notificationRepository;

    public MarkNotificationReadCommandHandler(IRepository<Domain.Entities.Notification> notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<Result<string>> Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        var notifications = await _notificationRepository.FindAsync(n => n.Id == request.NotificationId && n.UserId == request.UserId);
        var notif = notifications.FirstOrDefault();

        if (notif == null)
            return Result<string>.Failure(new[] { "Notification not found." });

        notif.IsRead = true;
        _notificationRepository.Update(notif);
        await _notificationRepository.SaveChangesAsync(cancellationToken);

        return Result<string>.Success("Marked as read.");
    }
}
