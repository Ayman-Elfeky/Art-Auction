using ArtAuction.Application.Common.Models;
using ArtAuction.Application.Features.Bids.DTOs;
using MediatR;

namespace ArtAuction.Application.Features.Bids.Queries.GetBidHistory;

public record GetBidHistoryQuery(Guid ArtworkId) : IRequest<Result<List<BidDto>>>;
