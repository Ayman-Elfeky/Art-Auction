using ArtAuction.Application.Common.Interfaces;
using ArtAuction.Application.Common.Models;
using ArtAuction.Application.Features.Bids.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArtAuction.Application.Features.Bids.Queries.GetBidHistory;

public class GetBidHistoryQueryHandler : IRequestHandler<GetBidHistoryQuery, Result<List<BidDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetBidHistoryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<BidDto>>> Handle(GetBidHistoryQuery request, CancellationToken cancellationToken)
    {
        var bids = await _context.Bids
            .Where(b => b.ArtworkId == request.ArtworkId)
            .Include(b => b.Buyer)
            .OrderByDescending(b => b.Amount)
            .ThenByDescending(b => b.PlacedAt)
            .Select(b => new BidDto
            {
                Id = b.Id.ToString(),
                BuyerId = b.BuyerId.ToString(),
                ArtworkId = b.ArtworkId.ToString(),
                Amount = b.Amount,
                IsWinning = b.IsWinning,
                PlacedAt = b.PlacedAt,
                BuyerName = b.Buyer != null ? b.Buyer.Username : null
            })
            .ToListAsync(cancellationToken);

        return Result<List<BidDto>>.Success(bids);
    }
}
