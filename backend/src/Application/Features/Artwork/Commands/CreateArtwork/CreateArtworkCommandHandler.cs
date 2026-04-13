using ArtAuction.Application.Common.Exceptions;
using ArtAuction.Application.Common.Interfaces;
using ArtAuction.Application.Common.Models;
using ArtAuction.Application.Features.Artworks.DTOs;
using ArtAuction.Domain.Entities;
using ArtAuction.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArtAuction.Application.Features.Artworks.Commands.CreateArtwork;

public class CreateArtworkCommandHandler : IRequestHandler<CreateArtworkCommand, Result<ArtworkDto>>
{
    private readonly IApplicationDbContext _context;

    public CreateArtworkCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ArtworkDto>> Handle(CreateArtworkCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate artist exists and is approved
        var artist = await _context.Users.FindAsync(new object[] { request.ArtistId }, cancellationToken);
        if (artist == null)
        {
            return Result<ArtworkDto>.Failure("Artist not found");
        }

        if (artist.Role != UserRole.Artist)
        {
            return Result<ArtworkDto>.Failure("User must be an artist to create artworks");
        }

        if (!artist.IsApproved)
        {
            return Result<ArtworkDto>.Failure("Artist account must be approved before creating artworks");
        }

        // Prevent duplicate artworks for the same artist by title (case-insensitive).
        var normalizedTitle = request.Dto.Title.Trim().ToLowerInvariant();
        var exists = await _context.Artworks
            .AnyAsync(a => a.ArtistId == request.ArtistId && a.Title.ToLower() == normalizedTitle, cancellationToken);
        if (exists)
        {
            return Result<ArtworkDto>.Failure("Artwork with the same title already exists for this artist");
        }

        // 2. Resolve (or create) category by name
        var normalizedCategoryName = request.Dto.CategoryName.Trim();
        var normalizedLookup = normalizedCategoryName.ToLowerInvariant();
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Name.ToLower() == normalizedLookup, cancellationToken);

        if (category is null)
        {
            category = new Category
            {
                Id = Guid.NewGuid(),
                Name = normalizedCategoryName,
                Description = string.Empty
            };

            _context.Categories.Add(category);
        }

        // 3. Create artwork with Pending status
        var artwork = new Artwork
        {
            Id = Guid.NewGuid(),
            Title = request.Dto.Title,
            Description = request.Dto.Description,
            ArtistId = request.ArtistId,
            CategoryId = category.Id,
            InitialPrice = request.Dto.InitialPrice,
            BuyNowPrice = request.Dto.BuyNowPrice,
            CurrentBid = request.Dto.InitialPrice,
            AuctionStartTime = request.Dto.AuctionStartTime,
            AuctionEndTime = request.Dto.AuctionEndTime,
            Status = ArtworkStatus.Pending,
            ImageUrl = request.Dto.ImageUrl,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // 4. Add tags if provided
        if (request.Dto.Tags.Any())
        {
            foreach (var tag in request.Dto.Tags)
            {
                artwork.Tags.Add(new ArtworkTag { Tag = tag, ArtworkId = artwork.Id });
            }
        }

        _context.Artworks.Add(artwork);
        await _context.SaveChangesAsync(cancellationToken);

        var artworkDto = new ArtworkDto
        {
            Id = artwork.Id,
            Title = artwork.Title,
            ArtistName = artist.Username,
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
