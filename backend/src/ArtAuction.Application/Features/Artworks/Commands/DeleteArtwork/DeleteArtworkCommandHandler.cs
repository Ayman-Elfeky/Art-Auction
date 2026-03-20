using ArtAuction.Application.Common.Exceptions;
using ArtAuction.Application.Common.Interfaces;
using ArtAuction.Application.Common.Models;
using ArtAuction.Domain.Entities;
using ArtAuction.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArtAuction.Application.Features.Artworks.Commands.DeleteArtwork;

public class DeleteArtworkCommandHandler
    : IRequestHandler<DeleteArtworkCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public DeleteArtworkCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(
        DeleteArtworkCommand request,
        CancellationToken cancellationToken)
    {
        var artwork = await _context.Artworks
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (artwork == null)
            throw new NotFoundException(nameof(Artwork), request.Id);

        if (artwork.ArtistId != request.ArtistId)
            throw new ForbiddenException();

        // Cannot delete active auction
        if (artwork.Status == ArtworkStatus.Active)
            return Result<bool>.Failure(
                "Cannot delete an active auction.");

        _context.Artworks.Remove(artwork);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}   