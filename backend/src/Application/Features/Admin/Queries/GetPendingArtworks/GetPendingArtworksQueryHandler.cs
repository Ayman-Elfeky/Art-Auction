using ArtAuction.Application.Common.Interfaces;
using ArtAuction.Application.Features.Admin.DTOs;
using ArtAuction.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArtAuction.Application.Features.Admin.Queries.GetPendingArtworks
{
    public class GetPendingArtworksQueryHandler
        : IRequestHandler<GetPendingArtworksQuery, List<PendingArtworkDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetPendingArtworksQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<PendingArtworkDto>> Handle(
            GetPendingArtworksQuery request,
            CancellationToken cancellationToken)
        {
            return await _context.Artworks
                .Include(a => a.Artist)   // JOIN with Users table to get artist name
                .Where(a => a.Status == ArtworkStatus.Pending)
                .Select(a => new PendingArtworkDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    InitialPrice = a.InitialPrice,
                    ArtistName = a.Artist.Username,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync(cancellationToken);
        }
    }
}