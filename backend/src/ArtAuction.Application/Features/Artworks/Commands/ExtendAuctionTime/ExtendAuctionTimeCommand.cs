using ArtAuction.Application.Common.Models;
using MediatR;

namespace ArtAuction.Application.Features.Artworks.Commands.ExtendAuctionTime;

public class ExtendAuctionTimeCommand : IRequest<Result<bool>>
{
    public Guid ArtworkId { get; set; }
    public Guid ArtistId { get; set; }
    public DateTime NewEndTime { get; set; }
}