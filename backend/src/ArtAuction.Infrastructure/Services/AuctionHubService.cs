using ArtAuction.Application.Common.Interfaces;

namespace ArtAuction.Infrastructure.Services;

public class AuctionHubService : IAuctionHubService
{
    // TODO: Implement SignalR connection management
    
    public async Task BroadcastNewBidAsync(string artworkId, object bidData)
    {
        await Task.Delay(100);
        // Will be implemented with SignalR
    }

    public async Task BroadcastAuctionEndedAsync(string artworkId, object winnerData)
    {
        await Task.Delay(100);
        // Will be implemented with SignalR
    }

    public async Task NotifyUserAsync(string userId, string title, string message)
    {
        await Task.Delay(100);
        // Will be implemented with SignalR
    }
}
