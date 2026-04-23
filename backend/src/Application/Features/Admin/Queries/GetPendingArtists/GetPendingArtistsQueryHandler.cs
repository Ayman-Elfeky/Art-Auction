using ArtAuction.Application.Common.Interfaces;
using ArtAuction.Application.Features.Admin.DTOs;
using ArtAuction.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArtAuction.Application.Features.Admin.Queries.GetPendingArtists
{
    public class GetPendingArtistsQueryHandler
        : IRequestHandler<GetPendingArtistsQuery, List<PendingArtistDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetPendingArtistsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<PendingArtistDto>> Handle(
            GetPendingArtistsQuery request,
            CancellationToken cancellationToken)
        {
            // Artists are Users with Role = Artist AND IsApproved = false
            return await _context.Users
                .Where(u => u.Role == UserRole.Artist && u.IsApproved == false)
                .Select(u => new PendingArtistDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync(cancellationToken);
        }
    }
}