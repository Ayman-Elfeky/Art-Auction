using ArtAuction.Application.Common.Interfaces;
using ArtAuction.Application.Common.Models;
using ArtAuction.Application.Features.Artworks.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArtAuction.Application.Features.Artworks.Queries.GetArtworksByArtist;

public class GetArtworksByArtistQueryHandler : IRequestHandler<GetArtworksByArtistQuery, PagedResult<ArtworkDto>>
{
    private readonly IApplicationDbContext _context;

    public GetArtworksByArtistQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ArtworkDto>> Handle(GetArtworksByArtistQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Artworks
            .Where(a => a.ArtistId == request.ArtistId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Include(a => a.Artist)
            .Include(a => a.Category)
            .OrderByDescending(a => a.CreatedAt)
            .Skip((request.PaginationParams.PageNumber - 1) * request.PaginationParams.PageSize)
            .Take(request.PaginationParams.PageSize)
            .Select(a => new ArtworkDto
            {
                Id = a.Id,
                Title = a.Title,
                ArtistName = a.Artist.Username,
                InitialPrice = a.InitialPrice,
                CurrentBid = a.CurrentBid,
                AuctionEndTime = a.AuctionEndTime,
                Status = a.Status.ToString(),
                ImageUrl = a.ImageUrl
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<ArtworkDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PaginationParams.PageNumber,
            PageSize = request.PaginationParams.PageSize
        };
    }
}
