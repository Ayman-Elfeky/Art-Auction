using ArtAuction.Application.Common.Models;
using ArtAuction.Application.Features.Artworks.DTOs;
using MediatR;

namespace ArtAuction.Application.Features.Artworks.Queries.GetArtworksByArtist;

public class GetArtworksByArtistQuery : IRequest<PagedResult<ArtworkDto>>
{
    public Guid ArtistId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}   