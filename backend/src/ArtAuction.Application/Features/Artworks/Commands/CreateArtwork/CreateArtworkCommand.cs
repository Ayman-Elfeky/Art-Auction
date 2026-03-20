using ArtAuction.Application.Common.Models;
using ArtAuction.Application.Features.Artworks.DTOs;
using MediatR;

namespace ArtAuction.Application.Features.Artworks.Commands.CreateArtwork;

public class CreateArtworkCommand : IRequest<Result<ArtworkDto>>
{
    public Guid ArtistId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal InitialPrice { get; set; }
    public decimal? BuyNowPrice { get; set; }
    public DateTime AuctionStartTime { get; set; }
    public DateTime AuctionEndTime { get; set; }
    public Guid CategoryId { get; set; }
    public List<string> Tags { get; set; } = new();
    public string ImageUrl { get; set; } = string.Empty;
}