using ArtAuction.Application.Common.Models;

namespace ArtAuction.Application.Features.Artworks.Queries.GetArtworks;

public class ArtworkFilterParams : PaginationParams
{
    public string? ArtistName { get; set; }
    public string? CategoryName { get; set; }
    public string? TagName { get; set; }
    public string? Status { get; set; }
}
