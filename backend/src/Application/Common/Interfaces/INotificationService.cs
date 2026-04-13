using ArtAuction.Domain.Enums;

namespace ArtAuction.Application.Common.Interfaces;

public interface INotificationService
{
    Task SendNotificationAsync(
        Guid userId,
        string title,
        string message,
        NotificationType type,
        Guid? relatedArtworkId);
}
