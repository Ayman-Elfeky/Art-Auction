using MediatR;

namespace ArtAuction.Application.Features.Admin.Commands.RejectArtistAccount
{
    public class RejectArtistAccountCommand : IRequest<bool>
    {
        public Guid ArtistId { get; set; }

        public RejectArtistAccountCommand(Guid artistId)
        {
            ArtistId = artistId;
        }
    }
}