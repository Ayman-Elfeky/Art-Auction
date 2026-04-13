using MediatR;

namespace ArtAuction.Application.Features.Admin.Commands.RejectArtwork
{
    public class RejectArtworkCommand : IRequest<bool>
    {
        public Guid ArtworkId { get; set; }

        public RejectArtworkCommand(Guid artworkId)
        {
            ArtworkId = artworkId;
        }
    }
}