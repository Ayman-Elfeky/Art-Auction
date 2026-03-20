namespace ArtAuction.Domain.Entities;

public class Watchlist
{
    public Guid Id { get; set; }
    public Guid BuyerId { get; set; }
    public Guid ArtworkId { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User Buyer { get; set; } = null!;
    public Artwork Artwork { get; set; } = null!;
}