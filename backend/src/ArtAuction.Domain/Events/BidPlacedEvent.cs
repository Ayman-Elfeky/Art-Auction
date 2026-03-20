namespace ArtAuction.Domain.Events;

public class BidPlacedEvent
{
    public Guid ArtworkId { get; set; }
    public Guid BuyerId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PlacedAt { get; set; }

    public BidPlacedEvent(Guid artworkId, Guid buyerId, decimal amount)
    {
        ArtworkId = artworkId;
        BuyerId = buyerId;
        Amount = amount;
        PlacedAt = DateTime.UtcNow;
    }
}