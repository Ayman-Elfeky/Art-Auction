using ArtAuction.Application.Common.Models;
using ArtAuction.Application.Features.Artworks.DTOs;
using MediatR;

namespace ArtAuction.Application.Features.Artworks.Queries.GetArtworkById;

public class GetArtworkByIdQuery : IRequest<Result<ArtworkDetailDto>>
{
    public Guid Id { get; set; }
}