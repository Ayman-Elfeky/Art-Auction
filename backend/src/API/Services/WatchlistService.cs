using ArtAuction.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using VeldGenerated.Models;

namespace Api.Services;

public interface IWatchlistService
{
    Task<string> AddToWatchlist(string artworkId);
    Task<SuccessMessage> RemoveFromWatchlist(string artworkId);
    Task<List<Artwork>> GetMyWatchlist();
}

public sealed class WatchlistService : IWatchlistService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserContext _currentUserContext;

    public WatchlistService(ApplicationDbContext context, ICurrentUserContext currentUserContext)
    {
        _context = context;
        _currentUserContext = currentUserContext;
    }

    public async Task<string> AddToWatchlist(string artworkId)
    {
        var buyerId = _currentUserContext.GetRequiredUserId();
        if (!Guid.TryParse(artworkId, out var parsedArtworkId))
        {
            throw new InvalidOperationException("Invalid artwork id.");
        }

        var artworkExists = await _context.Artworks.AnyAsync(a => a.Id == parsedArtworkId);
        if (!artworkExists)
        {
            throw new InvalidOperationException("Artwork not found.");
        }

        var exists = await _context.Watchlists.AnyAsync(w => w.BuyerId == buyerId && w.ArtworkId == parsedArtworkId);
        if (exists)
        {
            return "Artwork is already in watchlist.";
        }

        _context.Watchlists.Add(new ArtAuction.Domain.Entities.Watchlist
        {
            Id = Guid.NewGuid(),
            BuyerId = buyerId,
            ArtworkId = parsedArtworkId,
            AddedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
        return "Artwork added to watchlist.";
    }

    public async Task<SuccessMessage> RemoveFromWatchlist(string artworkId)
    {
        var buyerId = _currentUserContext.GetRequiredUserId();
        if (!Guid.TryParse(artworkId, out var parsedArtworkId))
        {
            throw new InvalidOperationException("Invalid artwork id.");
        }

        var item = await _context.Watchlists
            .FirstOrDefaultAsync(w => w.BuyerId == buyerId && w.ArtworkId == parsedArtworkId);
        if (item is null)
        {
            return new SuccessMessage(true, "Artwork is not in watchlist.");
        }

        _context.Watchlists.Remove(item);
        await _context.SaveChangesAsync();
        return new SuccessMessage(true, "Artwork removed from watchlist.");
    }

    public async Task<List<Artwork>> GetMyWatchlist()
    {
        var buyerId = _currentUserContext.GetRequiredUserId();
        var items = await _context.Watchlists
            .Where(w => w.BuyerId == buyerId)
            .Include(w => w.Artwork)
            .ThenInclude(a => a.Artist)
            .Include(w => w.Artwork.Category)
            .OrderByDescending(w => w.AddedAt)
            .Select(w => new Artwork(
                w.Artwork.Id,
                w.Artwork.Title,
                w.Artwork.Artist.Username,
                w.Artwork.Category.Name,
                (double)w.Artwork.InitialPrice,
                (double)w.Artwork.CurrentBid,
                w.Artwork.AuctionEndTime,
                w.Artwork.Status.ToString(),
                w.Artwork.ImageUrl))
            .ToListAsync();

        return items;
    }
}
