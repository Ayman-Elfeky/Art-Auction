using ArtAuction.Application.Features.Admin.DTOs;
using MediatR;

namespace ArtAuction.Application.Features.Admin.Queries.GetPendingArtworks
{
    public class GetPendingArtworksQuery : IRequest<List<PendingArtworkDto>>
    {
    }
}