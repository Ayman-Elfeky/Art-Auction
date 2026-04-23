namespace ArtAuction.Domain.Events;

public class AuctionEndedEvent
{
    public Guid ArtworkId { get; set; }
    public Guid? WinnerBuyerId { get; set; }
    public decimal? WinningAmount { get; set; }
    public DateTime EndedAt { get; set; }

    public AuctionEndedEvent(Guid artworkId, Guid? winnerBuyerId, decimal? winningAmount)
    {
        ArtworkId = artworkId;
        WinnerBuyerId = winnerBuyerId;
        WinningAmount = winningAmount;
        EndedAt = DateTime.UtcNow;
    }
}