using Api.Hubs;
using ArtAuction.Application.Common.Interfaces;
using ArtAuction.Domain.Entities;
using ArtAuction.Domain.Enums;
using ArtAuction.Infrastructure.Persistence;
using Microsoft.AspNetCore.SignalR;

namespace Api.Services;

public sealed class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<AuctionHub> _hubContext;

    public NotificationService(ApplicationDbContext context, IHubContext<AuctionHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    public async Task SendNotificationAsync(
        Guid userId,
        string title,
        string message,
        NotificationType type,
        Guid? relatedArtworkId)
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

        await _context.Notifications.AddAsync(notification);
        await _context.SaveChangesAsync();

        await _hubContext.Clients.Group(AuctionHub.GetUserGroup(userId.ToString()))
            .SendAsync("notification.received", new
            {
                id = notification.Id,
                title = notification.Title,
                message = notification.Message,
                type = notification.Type.ToString(),
                relatedArtworkId = notification.RelatedArtworkId,
                createdAt = notification.CreatedAt
            });
    }
}
