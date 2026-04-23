using ArtAuction.Application.Common.Models;
using ArtAuction.Application.Features.Artworks.DTOs;
using MediatR;

namespace ArtAuction.Application.Features.Artworks.Commands.UpdateArtwork;

public record UpdateArtworkCommand(Guid ArtworkId, CreateArtworkDto Dto, Guid UserId) : IRequest<Result<ArtworkDto>>;
