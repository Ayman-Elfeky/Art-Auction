using ArtAuction.Application.Common.Interfaces;
using ArtAuction.Application.Common.Models;
using ArtAuction.Application.Features.Artworks.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArtAuction.Application.Features.Artworks.Queries.GetArtworks;

public class GetArtworksQueryHandler : IRequestHandler<GetArtworksQuery, PagedResult<ArtworkDto>>
{
    private readonly IApplicationDbContext _context;

    public GetArtworksQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ArtworkDto>> Handle(GetArtworksQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Artworks.AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(request.FilterParams.ArtistName))
        {
            query = query.Where(a => a.Artist.Username.Contains(request.FilterParams.ArtistName));
        }

        if (!string.IsNullOrEmpty(request.FilterParams.CategoryName))
        {
            query = query.Where(a => a.Category.Name == request.FilterParams.CategoryName);
        }

        if (!string.IsNullOrEmpty(request.FilterParams.TagName))
        {
            query = query.Where(a => a.Tags.Any(t => t.Tag == request.FilterParams.TagName));
        }

        if (!string.IsNullOrEmpty(request.FilterParams.Status))
        {
            query = query.Where(a => a.Status.ToString() == request.FilterParams.Status);
        }
        else
        {
            query = query.Where(a => a.Status != Domain.Enums.ArtworkStatus.Pending && a.Status != Domain.Enums.ArtworkStatus.Rejected);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var items = await query
            .Include(a => a.Artist)
            .Include(a => a.Category)
            .OrderByDescending(a => a.CreatedAt)
            .Skip((request.FilterParams.PageNumber - 1) * request.FilterParams.PageSize)
            .Take(request.FilterParams.PageSize)
            .Select(a => new ArtworkDto
            {
                Id = a.Id,
                Title = a.Title,
                ArtistName = a.Artist.Username,
                CategoryName = a.Category.Name,
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
            PageNumber = request.FilterParams.PageNumber,
            PageSize = request.FilterParams.PageSize
        };
    }
}
