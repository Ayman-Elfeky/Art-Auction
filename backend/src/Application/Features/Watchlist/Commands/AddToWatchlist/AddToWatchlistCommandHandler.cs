using ArtAuction.Application.Common.Interfaces;
using ArtAuction.Application.Common.Models;
using ArtAuction.Domain.Entities;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ArtAuction.Application.Features.Watchlist.Commands.AddToWatchlist;

public class AddToWatchlistCommandHandler : IRequestHandler<AddToWatchlistCommand, Result<string>>
{
    private readonly IRepository<Domain.Entities.Watchlist> _watchlistRepository;
    private readonly IRepository<Domain.Entities.Artwork> _artworkRepository;

    public AddToWatchlistCommandHandler(
        IRepository<Domain.Entities.Watchlist> watchlistRepository,
        IRepository<Domain.Entities.Artwork> artworkRepository)
    {
        _watchlistRepository = watchlistRepository;
        _artworkRepository = artworkRepository;
    }

    public async Task<Result<string>> Handle(AddToWatchlistCommand request, CancellationToken cancellationToken)
    {
        var artwork = await _artworkRepository.GetByIdAsync(request.ArtworkId);
        if (artwork == null)
            return Result<string>.Failure(new[] { "Artwork not found." });

        var existingWatchlists = await _watchlistRepository.FindAsync(w => w.BuyerId == request.BuyerId && w.ArtworkId == request.ArtworkId);
        if (existingWatchlists.Count > 0)
            return Result<string>.Success("Artwork is already in watchlist.");

        var watchlist = new Domain.Entities.Watchlist
        {
            Id = Guid.NewGuid(),
            BuyerId = request.BuyerId,
            ArtworkId = request.ArtworkId,
            AddedAt = DateTime.UtcNow
        };

        await _watchlistRepository.AddAsync(watchlist, cancellationToken);
        await _watchlistRepository.SaveChangesAsync(cancellationToken);

        return Result<string>.Success("Artwork added to watchlist.");
    }
}
