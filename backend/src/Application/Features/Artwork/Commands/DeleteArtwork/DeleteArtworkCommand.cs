using ArtAuction.Application.Common.Models;
using MediatR;

namespace ArtAuction.Application.Features.Artworks.Commands.DeleteArtwork;

public record DeleteArtworkCommand(Guid ArtworkId, Guid UserId) : IRequest<Result>;
