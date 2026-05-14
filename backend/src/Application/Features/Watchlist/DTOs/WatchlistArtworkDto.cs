namespace ArtAuction.Application.Features.Watchlist.DTOs;

public record WatchlistArtworkDto(
    Guid Id,
    string Title,
    string ArtistName,
    string CategoryName,
    double InitialPrice,
    double CurrentBid,
    DateTime AuctionEndTime,
    string Status,
    string ImageUrl
);
