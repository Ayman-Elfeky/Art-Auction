using ArtAuction.Application.Common.Models;
using MediatR;

namespace ArtAuction.Application.Features.Artworks.Commands.ApproveArtwork;

public class ApproveArtworkCommand : IRequest<Result<bool>>
{
    public Guid ArtworkId { get; set; }
}