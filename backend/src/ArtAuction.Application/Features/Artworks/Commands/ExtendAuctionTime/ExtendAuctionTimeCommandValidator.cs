using FluentValidation;

namespace ArtAuction.Application.Features.Artworks.Commands.ExtendAuctionTime;

public class ExtendAuctionTimeCommandValidator
    : AbstractValidator<ExtendAuctionTimeCommand>
{
    public ExtendAuctionTimeCommandValidator()
    {
        RuleFor(x => x.NewEndTime)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("New end time must be in the future.");
    }
}