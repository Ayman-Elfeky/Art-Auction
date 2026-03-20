using ArtAuction.Application.Common.Models;
using MediatR;

namespace ArtAuction.Application.Features.Artworks.Commands.DeleteArtwork;

public class DeleteArtworkCommand : IRequest<Result<bool>>
{
    public Guid Id { get; set; }
    public Guid ArtistId { get; set; }
}