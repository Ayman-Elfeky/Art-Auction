using ArtAuction.Application.Common.Models;
using MediatR;

namespace ArtAuction.Application.Features.Watchlist.Commands.RemoveFromWatchlist;

public record RemoveFromWatchlistCommand(Guid ArtworkId, Guid BuyerId) : IRequest<Result<string>>;
