using ArtAuction.Application.Common.Models;
using ArtAuction.Application.Features.Artworks.DTOs;
using MediatR;

namespace ArtAuction.Application.Features.Artworks.Commands.ExtendAuctionTime;

public record ExtendAuctionTimeCommand(Guid ArtworkId, DateTime NewEndTime, Guid UserId) : IRequest<Result<ArtworkDto>>;
