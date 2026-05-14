using ArtAuction.Application.Common.Interfaces;
using ArtAuction.Application.Features.Watchlist.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ArtAuction.Application.Features.Watchlist.Queries.GetWatchlist;

public class GetWatchlistQueryHandler : IRequestHandler<GetWatchlistQuery, List<WatchlistArtworkDto>>
{
    private readonly IApplicationDbContext _context;

    public GetWatchlistQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<WatchlistArtworkDto>> Handle(GetWatchlistQuery request, CancellationToken cancellationToken)
    {
        return await _context.Watchlists
            .Where(w => w.BuyerId == request.BuyerId)
            .Include(w => w.Artwork).ThenInclude(a => a.Artist)
            .Include(w => w.Artwork.Category)
            .OrderByDescending(w => w.AddedAt)
            .Select(w => new WatchlistArtworkDto(
                w.Artwork.Id,
                w.Artwork.Title,
                w.Artwork.Artist.Username,
                w.Artwork.Category.Name,
                (double)w.Artwork.InitialPrice,
                (double)w.Artwork.CurrentBid,
                w.Artwork.AuctionEndTime,
                w.Artwork.Status.ToString(),
                w.Artwork.ImageUrl))
            .ToListAsync(cancellationToken);
    }
}
