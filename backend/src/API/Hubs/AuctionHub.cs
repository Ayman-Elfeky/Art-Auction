using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Api.Hubs;

public class AuctionHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? Context.User?.FindFirstValue("sub");

        if (!string.IsNullOrWhiteSpace(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GetUserGroup(userId));
        }

        await base.OnConnectedAsync();
    }

    public Task JoinArtworkGroup(string artworkId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, GetArtworkGroup(artworkId));
    }

    public Task LeaveArtworkGroup(string artworkId)
    {
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, GetArtworkGroup(artworkId));
    }

    [Authorize]
    public Task JoinMyNotifications()
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? Context.User?.FindFirstValue("sub")
            ?? throw new HubException("Unauthenticated user.");

        return Groups.AddToGroupAsync(Context.ConnectionId, GetUserGroup(userId));
    }

    public static string GetArtworkGroup(string artworkId) => $"artwork-{artworkId}";
    public static string GetUserGroup(string userId) => $"user-{userId}";
}
