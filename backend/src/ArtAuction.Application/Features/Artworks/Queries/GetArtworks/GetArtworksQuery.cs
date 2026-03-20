using ArtAuction.Application.Common.Models;
using ArtAuction.Application.Features.Artworks.DTOs;
using MediatR;

namespace ArtAuction.Application.Features.Artworks.Queries.GetArtworks;

public class GetArtworksQuery : IRequest<PagedResult<ArtworkDto>>
{
    public ArtworkFilterParams Filters { get; set; } = new();
}