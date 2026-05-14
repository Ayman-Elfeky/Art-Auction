using ArtAuction.Application.Common.Interfaces;
using ArtAuction.Application.Common.Models;
using ArtAuction.Domain.Entities;
using ArtAuction.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArtAuction.Application.Features.Bids.Commands.PlaceBid;

public class PlaceBidCommandHandler : IRequestHandler<PlaceBidCommand, Result<PlaceBidResult>>
{
    private readonly IApplicationDbContext _context;

    public PlaceBidCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PlaceBidResult>> Handle(PlaceBidCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Dto.ArtId, out var artworkId))
        {
            return Result<PlaceBidResult>.Failure("Invalid artwork id.");
        }

        var buyer = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.BuyerId, cancellationToken);
        if (buyer is null || buyer.Role != UserRole.Buyer || !buyer.IsActive)
            return Result<PlaceBidResult>.Failure("Only active buyers can place bids.");

        var artwork = await _context.Artworks.Include(a => a.Artist).FirstOrDefaultAsync(a => a.Id == artworkId, cancellationToken);
        if (artwork == null)
            return Result<PlaceBidResult>.Failure("Artwork not found.");

        if (artwork.Status is ArtworkStatus.Pending or ArtworkStatus.Rejected or ArtworkStatus.Ended)
            return Result<PlaceBidResult>.Failure("Bidding is not allowed for this artwork.");

        var now = DateTime.UtcNow;
        if (now < artwork.AuctionStartTime || now > artwork.AuctionEndTime)
            return Result<PlaceBidResult>.Failure("Bidding is allowed only during the auction period.");

        var highestBid = await _context.Bids.Where(b => b.ArtworkId == artworkId).OrderByDescending(b => b.Amount).FirstOrDefaultAsync(cancellationToken);
        var minimumAmount = highestBid is null ? artwork.InitialPrice : highestBid.Amount + 10m;

        if (request.Dto.Amount < minimumAmount)
            return Result<PlaceBidResult>.Failure($"Bid must be at least ${minimumAmount:F2}.");

        var bidEntity = new Bid
        {
            Id = Guid.NewGuid(),
            ArtworkId = artworkId,
            BuyerId = request.BuyerId,
            Amount = request.Dto.Amount,
            PlacedAt = DateTime.UtcNow,
            IsWinning = true
        };

        if (highestBid != null)
        {
            highestBid.IsWinning = false;
            _context.Bids.Update(highestBid);
        }

        artwork.CurrentBid = bidEntity.Amount;
        bool auctionEnded = false;
        
        if (artwork.BuyNowPrice.HasValue && bidEntity.Amount >= artwork.BuyNowPrice.Value)
        {
            artwork.Status = ArtworkStatus.Ended;
            auctionEnded = true;
        }
        else
        {
            artwork.Status = ArtworkStatus.Active;
        }

        await _context.Bids.AddAsync(bidEntity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<PlaceBidResult>.Success(new PlaceBidResult(
            bidEntity.Id.ToString(),
            auctionEnded,
            artworkId,
            request.BuyerId,
            buyer.Username,
            bidEntity.Amount,
            bidEntity.PlacedAt
        ));
    }
}
