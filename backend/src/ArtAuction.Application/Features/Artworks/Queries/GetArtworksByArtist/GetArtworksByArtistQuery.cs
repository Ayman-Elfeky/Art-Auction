using ArtAuction.Application.Common.Models;
using ArtAuction.Application.Features.Artworks.DTOs;
using MediatR;

namespace ArtAuction.Application.Features.Artworks.Queries.GetArtworksByArtist;

public record GetArtworksByArtistQuery(Guid ArtistId, PaginationParams PaginationParams) : IRequest<PagedResult<ArtworkDto>>;
