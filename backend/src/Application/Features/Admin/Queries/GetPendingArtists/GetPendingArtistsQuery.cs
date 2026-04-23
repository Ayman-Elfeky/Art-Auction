using ArtAuction.Application.Features.Admin.DTOs;
using MediatR;

namespace ArtAuction.Application.Features.Admin.Queries.GetPendingArtists
{
    public class GetPendingArtistsQuery : IRequest<List<PendingArtistDto>>
    {
    }
}