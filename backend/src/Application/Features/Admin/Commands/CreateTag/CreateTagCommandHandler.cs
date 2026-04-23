using ArtAuction.Application.Common.Interfaces;
using ArtAuction.Application.Features.Admin.DTOs;
using ArtAuction.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArtAuction.Application.Features.Admin.Commands.CreateTag;

public class CreateTagCommandHandler
    : IRequestHandler<CreateTagCommand, TagDto>
{
    private readonly IApplicationDbContext _context;

    public CreateTagCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TagDto> Handle(
        CreateTagCommand request,
        CancellationToken cancellationToken)
    {
        // Check if tag already exists
        var exists = await _context.ArtworkTags
            .AnyAsync(t => t.Tag.ToLower() == request.Tag.ToLower(), cancellationToken);

        if (exists)
            throw new InvalidOperationException($"Tag '{request.Tag}' already exists.");

        var tag = new ArtworkTag
        {
            Id = Guid.NewGuid(),
            Tag = request.Tag
        };

        _context.ArtworkTags.Add(tag);
        await _context.SaveChangesAsync(cancellationToken);

        return new TagDto
        {
            Id = tag.Id,
            Tag = tag.Tag
        };
    }
}