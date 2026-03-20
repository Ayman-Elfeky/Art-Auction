using ArtAuction.Application.Common.Exceptions;
using ArtAuction.Application.Common.Interfaces;
using ArtAuction.Application.Common.Models;
using ArtAuction.Domain.Entities;
using ArtAuction.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArtAuction.Application.Features.Artworks.Commands.UpdateArtwork;

public class UpdateArtworkCommandHandler
    : IRequestHandler<UpdateArtworkCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public UpdateArtworkCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(
        UpdateArtworkCommand request,
        CancellationToken cancellationToken)
    {
        var artwork = await _context.Artworks
            .Include(a => a.Tags)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (artwork == null)
            throw new NotFoundException(nameof(Artwork), request.Id);

        // Only the owner can update
        if (artwork.ArtistId != request.ArtistId)
            throw new ForbiddenException();

        // Cannot update active or ended auctions
        if (artwork.Status == ArtworkStatus.Active ||
            artwork.Status == ArtworkStatus.Ended)
            return Result<bool>.Failure(
                "Cannot update an active or ended auction.");

        artwork.Title = request.Title;
        artwork.Description = request.Description;
        artwork.BuyNowPrice = request.BuyNowPrice;
        artwork.CategoryId = request.CategoryId;
        artwork.UpdatedAt = DateTime.UtcNow;

        // Replace tags
        _context.ArtworkTags.RemoveRange(artwork.Tags);
        foreach (var tag in request.Tags)
        {
            artwork.Tags.Add(new ArtworkTag
            {
                Id = Guid.NewGuid(),
                ArtworkId = artwork.Id,
                Tag = tag.ToLower().Trim()
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}   