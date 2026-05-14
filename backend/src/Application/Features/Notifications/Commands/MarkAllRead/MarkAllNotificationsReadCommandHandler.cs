using ArtAuction.Application.Common.Interfaces;
using ArtAuction.Application.Common.Models;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ArtAuction.Application.Features.Notifications.Commands.MarkAllRead;

public class MarkAllNotificationsReadCommandHandler : IRequestHandler<MarkAllNotificationsReadCommand, Result<string>>
{
    private readonly IRepository<Domain.Entities.Notification> _notificationRepository;

    public MarkAllNotificationsReadCommandHandler(IRepository<Domain.Entities.Notification> notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<Result<string>> Handle(MarkAllNotificationsReadCommand request, CancellationToken cancellationToken)
    {
        var unread = await _notificationRepository.FindAsync(n => n.UserId == request.UserId && !n.IsRead);

        foreach (var n in unread)
        {
            n.IsRead = true;
            _notificationRepository.Update(n);
        }
        await _notificationRepository.SaveChangesAsync(cancellationToken);

        return Result<string>.Success($"{unread.Count} notifications marked as read.");
    }
}
