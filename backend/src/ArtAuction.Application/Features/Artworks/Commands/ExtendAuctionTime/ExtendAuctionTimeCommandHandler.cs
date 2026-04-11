using ArtAuction.Application.Common.Exceptions;
using ArtAuction.Application.Common.Interfaces;
using ArtAuction.Application.Common.Models;
using ArtAuction.Application.Features.Artworks.DTOs;
using ArtAuction.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArtAuction.Application.Features.Artworks.Commands.ExtendAuctionTime;

public class ExtendAuctionTimeCommandHandler : IRequestHandler<ExtendAuctionTimeCommand, Result<ArtworkDto>>
{
    private readonly IApplicationDbContext _context;

    public ExtendAuctionTimeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ArtworkDto>> Handle(ExtendAuctionTimeCommand request, CancellationToken cancellationToken)
    {
        var artwork = await _context.Artworks
            .Include(a => a.Artist)
            .FirstOrDefaultAsync(a => a.Id == request.ArtworkId, cancellationToken);

        if (artwork == null)
        {
            return Result<ArtworkDto>.Failure("Artwork not found");
        }

        // Check ownership
        if (artwork.ArtistId != request.UserId)
        {
            return Result<ArtworkDto>.Failure("Can only extend your own artwork");
        }

        // Only works on active auctions
        if (artwork.Status != ArtworkStatus.Active)
        {
            return Result<ArtworkDto>.Failure("Can only extend active auctions");
        }

        // New end time must be after current end time
        if (request.NewEndTime <= artwork.AuctionEndTime)
        {
            return Result<ArtworkDto>.Failure("New end time must be after current end time");
        }

        artwork.AuctionEndTime = request.NewEndTime;
        artwork.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var artworkDto = new ArtworkDto
        {
            Id = artwork.Id,
            Title = artwork.Title,
            ArtistName = artwork.Artist.Username,
            InitialPrice = artwork.InitialPrice,
            CurrentBid = artwork.CurrentBid,
            AuctionEndTime = artwork.AuctionEndTime,
            Status = artwork.Status.ToString(),
            ImageUrl = artwork.ImageUrl
        };

        return Result<ArtworkDto>.Success(artworkDto);
    }
}
