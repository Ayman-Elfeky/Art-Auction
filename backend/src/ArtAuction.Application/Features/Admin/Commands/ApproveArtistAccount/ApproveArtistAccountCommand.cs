using MediatR;

namespace ArtAuction.Application.Features.Admin.Commands.ApproveArtistAccount
{
    public class ApproveArtistAccountCommand : IRequest<bool>
    {
        public Guid ArtistId { get; set; }

        public ApproveArtistAccountCommand(Guid artistId)
        {
            ArtistId = artistId;
        }
    }
}