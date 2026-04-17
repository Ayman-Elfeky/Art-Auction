using ArtAuction.Application.Common.Interfaces;
using ArtAuction.Application.Features.Admin.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArtAuction.Application.Features.Admin.Queries.GetAllTags;

public class GetAllTagsQueryHandler
    : IRequestHandler<GetAllTagsQuery, List<TagDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllTagsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TagDto>> Handle(
        GetAllTagsQuery request,
        CancellationToken cancellationToken)
    {
        // Tags in ArtworkTags table that have no ArtworkId
        // are the global tags created by admin
        return await _context.ArtworkTags
            .Where(t => t.ArtworkId == Guid.Empty)
            .Select(t => new TagDto
            {
                Id = t.Id,
                Tag = t.Tag
            })
            .ToListAsync(cancellationToken);
    }
}