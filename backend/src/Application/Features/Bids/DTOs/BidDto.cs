namespace ArtAuction.Application.Features.Bids.DTOs;

public class BidDto
{
    public string Id { get; set; } = string.Empty;
    public string BuyerId { get; set; } = string.Empty;
    public string ArtworkId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsWinning { get; set; }
    public DateTime PlacedAt { get; set; }
    public string? BuyerName { get; set; }
}
