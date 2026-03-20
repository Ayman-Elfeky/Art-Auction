using ArtAuction.Application.Common.Interfaces;
using ArtAuction.Application.Common.Models;
using ArtAuction.Application.Features.Artworks.DTOs;
using ArtAuction.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArtAuction.Application.Features.Artworks.Queries.GetArtworks;

public class GetArtworksQueryHandler
    : IRequestHandler<GetArtworksQuery, PagedResult<ArtworkDto>>
{
    private readonly IApplicationDbContext _context;

    public GetArtworksQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ArtworkDto>> Handle(
        GetArtworksQuery request,
        CancellationToken cancellationToken)
    {
        var filters = request.Filters;

        var query = _context.Artworks
            .Include(a => a.Artist)
            .Include(a => a.Category)
            .Include(a => a.Tags)
            .Where(a => a.Status == ArtworkStatus.Approved ||
                        a.Status == ArtworkStatus.Active ||
                        a.Status == ArtworkStatus.Ended)
            .AsQueryable();

        // Filter by artist name
        if (!string.IsNullOrWhiteSpace(filters.ArtistName))
            query = query.Where(a =>
                a.Artist.Username.ToLower()
                 .Contains(filters.ArtistName.ToLower()));

        // Filter by category
        if (!string.IsNullOrWhiteSpace(filters.Category))
            query = query.Where(a =>
                a.Category.Name.ToLower()
                 .Contains(filters.Category.ToLower()));

        // Filter by tag
        if (!string.IsNullOrWhiteSpace(filters.Tag))
            query = query.Where(a =>
                a.Tags.Any(t =>
                    t.Tag.ToLower().Contains(filters.Tag.ToLower())));

        // Filter by status
        if (!string.IsNullOrWhiteSpace(filters.Status) &&
            Enum.TryParse<ArtworkStatus>(filters.Status, out var status))
            query = query.Where(a => a.Status == status);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((filters.PageNumber - 1) * filters.PageSize)
            .Take(filters.PageSize)
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
            PageNumber = filters.PageNumber,
            PageSize = filters.PageSize
        };
    }
}