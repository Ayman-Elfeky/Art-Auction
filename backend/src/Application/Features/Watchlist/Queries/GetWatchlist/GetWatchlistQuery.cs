using ArtAuction.Application.Common.Models;
using ArtAuction.Application.Features.Watchlist.DTOs;
using MediatR;
using System;
using System.Collections.Generic;

namespace ArtAuction.Application.Features.Watchlist.Queries.GetWatchlist;

public record GetWatchlistQuery(Guid BuyerId) : IRequest<List<WatchlistArtworkDto>>;
