using ArtAuction.Application.Common.Exceptions;
using ArtAuction.Application.Common.Interfaces;
using ArtAuction.Application.Common.Models;
using ArtAuction.Domain.Entities;
using ArtAuction.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArtAuction.Application.Features.Artworks.Commands.ExtendAuctionTime;

public class ExtendAuctionTimeCommandHandler
    : IRequestHandler<ExtendAuctionTimeCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public ExtendAuctionTimeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(
        ExtendAuctionTimeCommand request,
        CancellationToken cancellationToken)
    {
        var artwork = await _context.Artworks
            .FirstOrDefaultAsync(a => a.Id == request.ArtworkId, cancellationToken);

        if (artwork == null)
            throw new NotFoundException(nameof(Artwork), request.ArtworkId);

        if (artwork.ArtistId != request.ArtistId)
            throw new ForbiddenException();

        // Can only extend active auctions
        if (artwork.Status != ArtworkStatus.Active)
            return Result<bool>.Failure(
                "Can only extend an active auction.");

        // New end time must be after current end time
        if (request.NewEndTime <= artwork.AuctionEndTime)
            return Result<bool>.Failure(
                "New end time must be after the current end time.");

        artwork.AuctionEndTime = request.NewEndTime;
        artwork.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}