using ArtAuction.Application.Common.Interfaces;
using ArtAuction.Domain.Enums;

namespace ArtAuction.Infrastructure.Notifications;

public sealed class NotificationService : INotificationService
{
    public Task SendNotificationAsync(
        Guid userId,
        string title,
        string message,
        NotificationType type,
        Guid? relatedArtworkId)
    {
        // TODO: persist/send notifications (email, websocket, etc.).
        // This placeholder unblocks DI and app startup.
        return Task.CompletedTask;
    }
}
