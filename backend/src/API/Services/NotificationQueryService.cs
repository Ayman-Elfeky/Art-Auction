using Api.Models;
using ArtAuction.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public interface INotificationQueryService
{
    Task<List<NotificationDto>> GetMyNotifications();
}

public sealed class NotificationQueryService : INotificationQueryService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserContext _currentUserContext;

    public NotificationQueryService(ApplicationDbContext context, ICurrentUserContext currentUserContext)
    {
        _context = context;
        _currentUserContext = currentUserContext;
    }

    public async Task<List<NotificationDto>> GetMyNotifications()
    {
        var userId = _currentUserContext.GetRequiredUserId();
        return await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationDto(
                n.Id,
                n.Title,
                n.Message,
                n.Type.ToString(),
                n.IsRead,
                n.CreatedAt,
                n.RelatedArtworkId))
            .ToListAsync();
    }
}
