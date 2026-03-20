using ArtAuction.Application.Common.Models;
using MediatR;

namespace ArtAuction.Application.Features.Artworks.Commands.RejectArtwork;

public class RejectArtworkCommand : IRequest<Result<bool>>
{
    public Guid ArtworkId { get; set; }
    public string Reason { get; set; } = string.Empty;
}