namespace ArtAuction.Domain.Entities;

public class ArtworkTag
{
    public Guid Id { get; set; }
    public Guid ArtworkId { get; set; }
    public string Tag { get; set; } = string.Empty;

    // Navigation properties
    public Artwork Artwork { get; set; } = null!;
}