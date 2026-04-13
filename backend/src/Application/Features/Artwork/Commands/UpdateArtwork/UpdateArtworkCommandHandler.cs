using ArtAuction.Application.Common.Exceptions;
using ArtAuction.Application.Common.Interfaces;
using ArtAuction.Application.Common.Models;
using ArtAuction.Application.Features.Artworks.DTOs;
using ArtAuction.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArtAuction.Application.Features.Artworks.Commands.UpdateArtwork;

public class UpdateArtworkCommandHandler : IRequestHandler<UpdateArtworkCommand, Result<ArtworkDto>>
{
    private readonly IApplicationDbContext _context;

    public UpdateArtworkCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ArtworkDto>> Handle(UpdateArtworkCommand request, CancellationToken cancellationToken)
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
            return Result<ArtworkDto>.Failure("Can only update your own artwork");
        }

        // Cannot update active auction
        if (artwork.Status == ArtworkStatus.Active)
        {
            return Result<ArtworkDto>.Failure("Cannot update active auction");
        }

        // Resolve (or create) category by name
        var normalizedCategoryName = request.Dto.CategoryName.Trim();
        var normalizedLookup = normalizedCategoryName.ToLowerInvariant();
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Name.ToLower() == normalizedLookup, cancellationToken);

        if (category is null)
        {
            category = new Domain.Entities.Category
            {
                Id = Guid.NewGuid(),
                Name = normalizedCategoryName,
                Description = string.Empty
            };

            _context.Categories.Add(category);
        }

        // Update fields
        artwork.Title = request.Dto.Title;
        artwork.Description = request.Dto.Description;
        artwork.InitialPrice = request.Dto.InitialPrice;
        artwork.BuyNowPrice = request.Dto.BuyNowPrice;
        artwork.AuctionStartTime = request.Dto.AuctionStartTime;
        artwork.AuctionEndTime = request.Dto.AuctionEndTime;
        artwork.CategoryId = category.Id;
        artwork.ImageUrl = request.Dto.ImageUrl;
        artwork.UpdatedAt = DateTime.UtcNow;

        // Update tags
        artwork.Tags.Clear();
        if (request.Dto.Tags.Any())
        {
            foreach (var tag in request.Dto.Tags)
            {
                artwork.Tags.Add(new Domain.Entities.ArtworkTag { Tag = tag, ArtworkId = artwork.Id });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        var artworkDto = new ArtworkDto
        {
            Id = artwork.Id,
            Title = artwork.Title,
            ArtistName = artwork.Artist.Username,
            CategoryName = category.Name,
            InitialPrice = artwork.InitialPrice,
            CurrentBid = artwork.CurrentBid,
            AuctionEndTime = artwork.AuctionEndTime,
            Status = artwork.Status.ToString(),
            ImageUrl = artwork.ImageUrl
        };

        return Result<ArtworkDto>.Success(artworkDto);
    }
}
