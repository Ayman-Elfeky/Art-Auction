namespace ArtAuction.Application.Common.Interfaces;

public interface IAuctionHubService
{
    Task BroadcastNewBidAsync(string artworkId, object bidData);
    Task BroadcastAuctionEndedAsync(string artworkId, object winnerData);
    Task NotifyUserAsync(string userId, string title, string message);
}