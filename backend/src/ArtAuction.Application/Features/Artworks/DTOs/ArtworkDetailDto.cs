namespace ArtAuction.Application.Features.Artworks.DTOs;

public class ArtworkDetailDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ArtistName { get; set; } = string.Empty;
    public Guid ArtistId { get; set; }
    public decimal InitialPrice { get; set; }
    public decimal? BuyNowPrice { get; set; }
    public decimal CurrentBid { get; set; }
    public DateTime AuctionStartTime { get; set; }
    public DateTime AuctionEndTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public int TotalBids { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
