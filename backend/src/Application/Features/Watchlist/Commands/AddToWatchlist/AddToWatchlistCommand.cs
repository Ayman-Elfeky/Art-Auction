using ArtAuction.Application.Common.Models;
using MediatR;

namespace ArtAuction.Application.Features.Watchlist.Commands.AddToWatchlist;

public record AddToWatchlistCommand(Guid ArtworkId, Guid BuyerId) : IRequest<Result<string>>;
