using ArtAuction.Application.Common.Models;
using MediatR;

namespace ArtAuction.Application.Features.Artworks.Commands.UpdateArtwork;

public class UpdateArtworkCommand : IRequest<Result<bool>>
{
    public Guid Id { get; set; }
    public Guid ArtistId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal? BuyNowPrice { get; set; }
    public Guid CategoryId { get; set; }
    public List<string> Tags { get; set; } = new();
}