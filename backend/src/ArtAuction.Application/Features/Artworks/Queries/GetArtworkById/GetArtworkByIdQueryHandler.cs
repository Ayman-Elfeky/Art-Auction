using ArtAuction.Application.Common.Exceptions;
using ArtAuction.Application.Common.Interfaces;
using ArtAuction.Application.Common.Models;
using ArtAuction.Application.Features.Artworks.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArtAuction.Application.Features.Artworks.Queries.GetArtworkById;

public class GetArtworkByIdQueryHandler
    : IRequestHandler<GetArtworkByIdQuery, Result<ArtworkDetailDto>>
{
    private readonly IApplicationDbContext _context;

    public GetArtworkByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ArtworkDetailDto>> Handle(
        GetArtworkByIdQuery request,
        CancellationToken cancellationToken)
    {
        var artwork = await _context.Artworks
            .Include(a => a.Artist)
            .Include(a => a.Category)
            .Include(a => a.Tags)
            .Include(a => a.Bids)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (artwork == null)
            throw new NotFoundException(nameof(artwork), request.Id);

        return Result<ArtworkDetailDto>.Success(new ArtworkDetailDto
        {
            Id = artwork.Id,
            ArtistId = artwork.ArtistId,
            ArtistName = artwork.Artist.Username,
            Title = artwork.Title,
            Description = artwork.Description,
            InitialPrice = artwork.InitialPrice,
            BuyNowPrice = artwork.BuyNowPrice,
            CurrentBid = artwork.CurrentBid,
            AuctionStartTime = artwork.AuctionStartTime,
            AuctionEndTime = artwork.AuctionEndTime,
            CategoryId = artwork.CategoryId.ToString(),
            CategoryName = artwork.Category.Name,
            Tags = artwork.Tags.Select(t => t.Tag).ToList(),
            Status = artwork.Status.ToString(),
            ImageUrl = artwork.ImageUrl,
            TotalBids = artwork.Bids.Count,
            CreatedAt = artwork.CreatedAt,
            UpdatedAt = artwork.UpdatedAt
        });
    }
}