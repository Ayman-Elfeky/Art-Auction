using ArtAuction.Application.Common.Exceptions;
using ArtAuction.Application.Common.Interfaces;
using ArtAuction.Application.Common.Models;
using ArtAuction.Application.Features.Artworks.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArtAuction.Application.Features.Artworks.Queries.GetArtworkById;

public class GetArtworkByIdQueryHandler : IRequestHandler<GetArtworkByIdQuery, Result<ArtworkDetailDto>>
{
    private readonly IApplicationDbContext _context;

    public GetArtworkByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ArtworkDetailDto>> Handle(GetArtworkByIdQuery request, CancellationToken cancellationToken)
    {
        var artwork = await _context.Artworks
            .Include(a => a.Artist)
            .Include(a => a.Category)
            .Include(a => a.Tags)
            .Include(a => a.Bids)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (artwork == null)
        {
            return Result<ArtworkDetailDto>.Failure("Artwork not found");
        }

        var detailDto = new ArtworkDetailDto
        {
            Id = artwork.Id,
            Title = artwork.Title,
            Description = artwork.Description,
            ArtistName = artwork.Artist.Username,
            ArtistId = artwork.Artist.Id,
            InitialPrice = artwork.InitialPrice,
            BuyNowPrice = artwork.BuyNowPrice,
            CurrentBid = artwork.CurrentBid,
            AuctionStartTime = artwork.AuctionStartTime,
            AuctionEndTime = artwork.AuctionEndTime,
            Status = artwork.Status.ToString(),
            CategoryName = artwork.Category.Name,
            Tags = artwork.Tags.Select(at => at.Tag).ToList(),
            TotalBids = artwork.Bids.Count,
            ImageUrl = artwork.ImageUrl,
            CreatedAt = artwork.CreatedAt
        };

        return Result<ArtworkDetailDto>.Success(detailDto);
    }
}
