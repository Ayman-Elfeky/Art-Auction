using Api.Services;
using ArtAuction.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public sealed class NotificationsController : ControllerBase
{
    private readonly INotificationQueryService _notificationQueryService;
    private readonly ApplicationDbContext _context;

    public NotificationsController(
        INotificationQueryService notificationQueryService,
        ApplicationDbContext context)
    {
        _notificationQueryService = notificationQueryService;
        _context = context;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyNotifications()
    {
        var result = await _notificationQueryService.GetMyNotifications();
        return Ok(result);
    }

    [HttpPut("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub");
        if (userIdStr is null || !Guid.TryParse(userIdStr, out var userId))
            return Unauthorized();

        var notif = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
        if (notif is null) return NotFound(new { error = "Notification not found." });

        notif.IsRead = true;
        await _context.SaveChangesAsync();
        return Ok(new { message = "Marked as read." });
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllRead()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub");
        if (userIdStr is null || !Guid.TryParse(userIdStr, out var userId))
            return Unauthorized();

        var unread = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var n in unread) n.IsRead = true;
        await _context.SaveChangesAsync();
        return Ok(new { message = $"{unread.Count} notifications marked as read." });
    }
}
