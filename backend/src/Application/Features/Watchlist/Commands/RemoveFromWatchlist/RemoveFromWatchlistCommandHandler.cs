using ArtAuction.Application.Common.Interfaces;
using ArtAuction.Application.Common.Models;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ArtAuction.Application.Features.Watchlist.Commands.RemoveFromWatchlist;

public class RemoveFromWatchlistCommandHandler : IRequestHandler<RemoveFromWatchlistCommand, Result<string>>
{
    private readonly IRepository<Domain.Entities.Watchlist> _watchlistRepository;

    public RemoveFromWatchlistCommandHandler(IRepository<Domain.Entities.Watchlist> watchlistRepository)
    {
        _watchlistRepository = watchlistRepository;
    }

    public async Task<Result<string>> Handle(RemoveFromWatchlistCommand request, CancellationToken cancellationToken)
    {
        var existingWatchlists = await _watchlistRepository.FindAsync(w => w.BuyerId == request.BuyerId && w.ArtworkId == request.ArtworkId);
        var item = existingWatchlists.FirstOrDefault();

        if (item == null)
            return Result<string>.Success("Artwork is not in watchlist.");

        _watchlistRepository.Remove(item);
        await _watchlistRepository.SaveChangesAsync(cancellationToken);

        return Result<string>.Success("Artwork removed from watchlist.");
    }
}
