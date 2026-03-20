using ArtAuction.Domain.Enums;

namespace ArtAuction.Domain.Entities;

public class Artwork
{
    public Guid Id { get; set; }
    public Guid ArtistId { get; set; }
    public Guid CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal InitialPrice { get; set; }
    public decimal? BuyNowPrice { get; set; }
    public decimal CurrentBid { get; set; }
    public DateTime AuctionStartTime { get; set; }
    public DateTime AuctionEndTime { get; set; }
    public ArtworkStatus Status { get; set; } = ArtworkStatus.Pending;
    public string ImageUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User Artist { get; set; } = null!;
    public Category Category { get; set; } = null!;
    public ICollection<ArtworkTag> Tags { get; set; } = new List<ArtworkTag>();
    public ICollection<Bid> Bids { get; set; } = new List<Bid>();
    public ICollection<Watchlist> Watchlist { get; set; } = new List<Watchlist>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}