using ArtAuction.Application.Common.Interfaces;
using ArtAuction.Application.Common.Models;
using ArtAuction.Application.Features.Artworks.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArtAuction.Application.Features.Artworks.Queries.GetArtworksByArtist;

public class GetArtworksByArtistQueryHandler
    : IRequestHandler<GetArtworksByArtistQuery, PagedResult<ArtworkDto>>
{
    private readonly IApplicationDbContext _context;

    public GetArtworksByArtistQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ArtworkDto>> Handle(
        GetArtworksByArtistQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Artworks
            .Include(a => a.Artist)
            .Include(a => a.Category)
            .Include(a => a.Tags)
            .Where(a => a.ArtistId == request.ArtistId)
            .AsQueryable();

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new ArtworkDto
            {
                Id = a.Id,
                ArtistId = a.ArtistId,
                ArtistName = a.Artist.Username,
                Title = a.Title,
                Description = a.Description,
                InitialPrice = a.InitialPrice,
                BuyNowPrice = a.BuyNowPrice,
                CurrentBid = a.CurrentBid,
                AuctionStartTime = a.AuctionStartTime,
                AuctionEndTime = a.AuctionEndTime,
                CategoryName = a.Category.Name,
                Tags = a.Tags.Select(t => t.Tag).ToList(),
                Status = a.Status.ToString(),
                ImageUrl = a.ImageUrl,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<ArtworkDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}