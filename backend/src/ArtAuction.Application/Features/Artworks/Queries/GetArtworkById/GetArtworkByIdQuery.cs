using ArtAuction.Application.Common.Models;
using ArtAuction.Application.Features.Artworks.DTOs;
using MediatR;

namespace ArtAuction.Application.Features.Artworks.Queries.GetArtworkById;

public record GetArtworkByIdQuery(Guid Id) : IRequest<Result<ArtworkDetailDto>>;
