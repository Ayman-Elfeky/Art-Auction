using ArtAuction.Application.Common.Interfaces;
using ArtAuction.Domain.Entities;
using ArtAuction.Domain.Enums;
using ArtAuction.Infrastructure.Persistence;

namespace ArtAuction.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;

    public NotificationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SendNotificationAsync(Guid userId, string title, string message, NotificationType type, Guid? relatedArtworkId = null)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            RelatedArtworkId = relatedArtworkId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
    }
}
