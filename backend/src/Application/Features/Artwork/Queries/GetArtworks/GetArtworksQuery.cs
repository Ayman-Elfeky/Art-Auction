using ArtAuction.Application.Common.Models;
using ArtAuction.Application.Features.Artworks.DTOs;
using MediatR;

namespace ArtAuction.Application.Features.Artworks.Queries.GetArtworks;

public record GetArtworksQuery(ArtworkFilterParams FilterParams) : IRequest<PagedResult<ArtworkDto>>;
