using ArtAuction.Application.Common.Models;
using ArtAuction.Application.Features.Bids.DTOs;
using MediatR;

namespace ArtAuction.Application.Features.Bids.Commands.PlaceBid;

public record PlaceBidResult(string BidId, bool AuctionEnded, Guid ArtworkId, Guid BuyerId, string BuyerName, decimal Amount, DateTime PlacedAt);

public record PlaceBidCommand(PlaceBidDto Dto, Guid BuyerId) : IRequest<Result<PlaceBidResult>>;
