using ArtAuction.Application.Common.Interfaces;
using ArtAuction.Application.Common.Models;
using ArtAuction.Application.Features.Artworks.DTOs;
using ArtAuction.Domain.Entities;
using ArtAuction.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArtAuction.Application.Features.Artworks.Commands.CreateArtwork;

public class CreateArtworkCommandHandler
    : IRequestHandler<CreateArtworkCommand, Result<ArtworkDto>>
{
    private readonly IApplicationDbContext _context;

    public CreateArtworkCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ArtworkDto>> Handle(
        CreateArtworkCommand request,
        CancellationToken cancellationToken)
    {
        // Verify category exists
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId, cancellationToken);

        if (category == null)
            return Result<ArtworkDto>.Failure("Category not found.");

        var artwork = new Artwork
        {
            Id = Guid.NewGuid(),
            ArtistId = request.ArtistId,
            Title = request.Title,
            Description = request.Description,
            InitialPrice = request.InitialPrice,
            BuyNowPrice = request.BuyNowPrice,
            CurrentBid = request.InitialPrice,
            AuctionStartTime = request.AuctionStartTime,
            AuctionEndTime = request.AuctionEndTime,
            CategoryId = request.CategoryId,
            Status = ArtworkStatus.Pending,
            ImageUrl = request.ImageUrl,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Add tags
        foreach (var tag in request.Tags)
        {
            artwork.Tags.Add(new ArtworkTag
            {
                Id = Guid.NewGuid(),
                ArtworkId = artwork.Id,
                Tag = tag.ToLower().Trim()
            });
        }

        _context.Artworks.Add(artwork);
        await _context.SaveChangesAsync(cancellationToken);

        // Fetch artist name for response
        var artist = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.ArtistId, cancellationToken);

        return Result<ArtworkDto>.Success(new ArtworkDto
        {
            Id = artwork.Id,
            ArtistId = artwork.ArtistId,
            ArtistName = artist?.Username ?? string.Empty,
            Title = artwork.Title,
            Description = artwork.Description,
            InitialPrice = artwork.InitialPrice,
            BuyNowPrice = artwork.BuyNowPrice,
            CurrentBid = artwork.CurrentBid,
            AuctionStartTime = artwork.AuctionStartTime,
            AuctionEndTime = artwork.AuctionEndTime,
            CategoryName = category.Name,
            Tags = request.Tags,
            Status = artwork.Status.ToString(),
            ImageUrl = artwork.ImageUrl,
            CreatedAt = artwork.CreatedAt
        });
    }
}