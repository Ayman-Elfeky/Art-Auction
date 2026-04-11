namespace ArtAuction.Application.Features.Artworks.DTOs;

public class ArtworkDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ArtistName { get; set; } = string.Empty;
    public decimal InitialPrice { get; set; }
    public decimal CurrentBid { get; set; }
    public DateTime AuctionEndTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
}
