namespace ArtAuction.Domain.Entities;

public class Bid
{
    public Guid Id { get; set; }
    public Guid ArtworkId { get; set; }
    public Guid BuyerId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PlacedAt { get; set; } = DateTime.UtcNow;
    public bool IsWinning { get; set; } = false;

    // Navigation properties
    public Artwork Artwork { get; set; } = null!;
    public User Buyer { get; set; } = null!;
}