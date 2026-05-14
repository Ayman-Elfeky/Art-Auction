namespace ArtAuction.Application.Features.Bids.DTOs;

public class PlaceBidDto
{
    public string ArtId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
