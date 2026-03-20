namespace ArtAuction.Application.Features.Artworks.Queries.GetArtworks;

public class ArtworkFilterParams
{
    public string? ArtistName { get; set; }
    public string? Category { get; set; }
    public string? Tag { get; set; }
    public string? Status { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}