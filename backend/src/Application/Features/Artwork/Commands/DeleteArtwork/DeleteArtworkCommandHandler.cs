using ArtAuction.Application.Common.Exceptions;
using ArtAuction.Application.Common.Interfaces;
using ArtAuction.Application.Common.Models;
using ArtAuction.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArtAuction.Application.Features.Artworks.Commands.DeleteArtwork;

public class DeleteArtworkCommandHandler : IRequestHandler<DeleteArtworkCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteArtworkCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteArtworkCommand request, CancellationToken cancellationToken)
    {
        var artwork = await _context.Artworks.FirstOrDefaultAsync(a => a.Id == request.ArtworkId, cancellationToken);

        if (artwork == null)
        {
            return Result.Failure("Artwork not found");
        }

        // Check ownership
        if (artwork.ArtistId != request.UserId)
        {
            return Result.Failure("Can only delete your own artwork");
        }

        // Cannot delete active auction
        if (artwork.Status == ArtworkStatus.Active)
        {
            return Result.Failure("Cannot delete active auction");
        }

        _context.Artworks.Remove(artwork);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
