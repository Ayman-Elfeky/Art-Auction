using ArtAuction.Application.Common.Models;
using ArtAuction.Application.Features.Artworks.DTOs;
using MediatR;

namespace ArtAuction.Application.Features.Artworks.Commands.CreateArtwork;

public record CreateArtworkCommand(CreateArtworkDto Dto, Guid ArtistId) : IRequest<Result<ArtworkDto>>;
