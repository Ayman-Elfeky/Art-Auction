using Api.Hubs;
using ArtAuction.Domain.Entities;
using ArtAuction.Domain.Enums;
using ArtAuction.Infrastructure.Persistence;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using VeldGenerated.Services;

namespace Api.Services;

public class BidService : IBidService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IHubContext<AuctionHub> _hubContext;

    public BidService(
        ApplicationDbContext context,
        ICurrentUserContext currentUserContext,
        IHubContext<AuctionHub> hubContext)
    {
        _context = context;
        _currentUserContext = currentUserContext;
        _hubContext = hubContext;
    }

    public async Task<string> CreateBid(VeldGenerated.Models.Bid input)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));

        var artworkId = Guid.Parse(input.ArtId);
        var buyerId = _currentUserContext.GetRequiredUserId();

        var buyer = await _context.Users.FirstOrDefaultAsync(u => u.Id == buyerId);
        if (buyer is null || buyer.Role != ArtAuction.Domain.Enums.UserRole.Buyer || !buyer.IsActive)
        {
            throw new InvalidOperationException("Only active buyers can place bids.");
        }

        var artwork = await _context.Artworks
            .Include(a => a.Artist)
            .FirstOrDefaultAsync(a => a.Id == artworkId)
            ?? throw new InvalidOperationException("Artwork not found.");

        if (artwork.Status is ArtworkStatus.Pending or ArtworkStatus.Rejected or ArtworkStatus.Ended)
        {
            throw new InvalidOperationException("Bidding is not allowed for this artwork.");
        }

        var now = DateTime.UtcNow;
        if (now < artwork.AuctionStartTime || now > artwork.AuctionEndTime)
        {
            throw new InvalidOperationException("Bidding is allowed only during the auction period.");
        }

        var highestBid = await _context.Bids
            .Where(b => b.ArtworkId == artworkId)
            .OrderByDescending(b => b.Amount)
            .FirstOrDefaultAsync();

        var minimumAmount = highestBid is null
            ? artwork.InitialPrice
            : highestBid.Amount + 10m;

        if (input.Amount < minimumAmount)
        {
            throw new InvalidOperationException($"Bid must be at least ${minimumAmount:F2}.");
        }

        var bidEntity = new ArtAuction.Domain.Entities.Bid
        {
            Id = Guid.NewGuid(),
            ArtworkId = artworkId,
            BuyerId = buyerId,
            Amount = input.Amount,
            PlacedAt = DateTime.UtcNow,
            IsWinning = true
        };

        if (highestBid != null)
        {
            highestBid.IsWinning = false;
            _context.Bids.Update(highestBid);
        }

        artwork.CurrentBid = bidEntity.Amount;
        
        // Handle 'Buy Now' logic
        if (artwork.BuyNowPrice.HasValue && bidEntity.Amount >= artwork.BuyNowPrice.Value)
        {
            artwork.Status = ArtworkStatus.Ended;
        }
        else
        {
            artwork.Status = ArtworkStatus.Active;
        }

        await _context.Bids.AddAsync(bidEntity);
        await _context.SaveChangesAsync();

        await _hubContext.Clients.Group(AuctionHub.GetArtworkGroup(artworkId.ToString()))
            .SendAsync("bid.placed", new
            {
                artworkId,
                bidderId = buyerId,
                bidderName = buyer.Username,
                amount = bidEntity.Amount,
                timestamp = bidEntity.PlacedAt
            });

        if (artwork.Status == ArtworkStatus.Ended)
        {
            await _hubContext.Clients.Group(AuctionHub.GetArtworkGroup(artworkId.ToString()))
                .SendAsync("auction.ended", new
                {
                    artworkId,
                    winnerId = buyerId,
                    winnerName = buyer.Username,
                    finalPrice = bidEntity.Amount
                });
        }

        return bidEntity.Id.ToString();
    }

    public async Task<List<VeldGenerated.Models.Bid>> GetBids(string Id)
    {
        var artworkId = Guid.Parse(Id);

        var bids = await _context.Bids
            .Where(b => b.ArtworkId == artworkId)
            .OrderByDescending(b => b.Amount)
            .ToListAsync();

        return bids
            .Select(b => new VeldGenerated.Models.Bid(
                b.Id.ToString(),
                b.BuyerId.ToString(),
                b.ArtworkId.ToString(),
                b.Amount,
                b.IsWinning,
                b.PlacedAt))
            .ToList();
    }
}