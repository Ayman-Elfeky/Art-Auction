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

        // 2. Verify category exists
        var category = await _context.Categories.FindAsync(new object[] { request.Dto.CategoryId }, cancellationToken);
        if (category == null)
        {
            return Result<ArtworkDto>.Failure("Category not found");
        }

        // 3. Create artwork with Pending status
        var artwork = new Artwork
        {
            Id = Guid.NewGuid(),
            Title = request.Dto.Title,
            Description = request.Dto.Description,
            ArtistId = request.ArtistId,
            CategoryId = request.Dto.CategoryId,
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
            InitialPrice = artwork.InitialPrice,
            CurrentBid = artwork.CurrentBid,
            AuctionEndTime = artwork.AuctionEndTime,
            Status = artwork.Status.ToString(),
            ImageUrl = artwork.ImageUrl
        };

        return Result<ArtworkDto>.Success(artworkDto);
    }
}
