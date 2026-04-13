using ArtAuction.Application.Mappings;
using ArtAuction.Domain.Entities;
using ArtAuction.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using VeldGenerated.Models;
using VeldGenerated.Services;

namespace ArtAuction.Application.Services;

public class BidService : IBidService
{
    private readonly ApplicationDbContext _context;

    public BidService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> CreateBid(VeldGenerated.Models.Bid input)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));

        var artworkId = Guid.Parse(input.artId);
        var buyerId = Guid.Parse(input.issuerId);

        // Get current highest bid
        var highestBid = await _context.Bids
            .Where(b => b.ArtworkId == artworkId)
            .OrderByDescending(b => b.Amount)
            .FirstOrDefaultAsync();

        if (highestBid != null && input.amount <= highestBid.Amount)
        {
            throw new Exception("Bid must be higher than the current highest bid.");
        }

        // Map to entity
        var bidEntity = BidMapper.ToEntity(input);
        bidEntity.Id = Guid.NewGuid();
        bidEntity.PlacedAt = DateTime.UtcNow;
        bidEntity.IsWinning = true;

        // Reset previous winning bid
        if (highestBid != null)
        {
            highestBid.IsWinning = false;
            _context.Bids.Update(highestBid);
        }

        await _context.Bids.AddAsync(bidEntity);
        await _context.SaveChangesAsync();

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
            .Select(BidMapper.ToModel)
            .ToList();
    }
}