using MediatR;

namespace ArtAuction.Application.Features.Admin.Commands.ApproveArtwork
{
    public class ApproveArtworkCommand : IRequest<bool>
    {
        public Guid ArtworkId { get; set; }

        public ApproveArtworkCommand(Guid artworkId)
        {
            ArtworkId = artworkId;
        }
    }
}