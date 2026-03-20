using FluentValidation;

namespace ArtAuction.Application.Features.Artworks.Commands.CreateArtwork;

public class CreateArtworkCommandValidator : AbstractValidator<CreateArtworkCommand>
{
    public CreateArtworkCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.");

        RuleFor(x => x.InitialPrice)
            .GreaterThan(0).WithMessage("Initial price must be greater than 0.");

        RuleFor(x => x.BuyNowPrice)
            .GreaterThan(x => x.InitialPrice)
            .When(x => x.BuyNowPrice.HasValue)
            .WithMessage("Buy now price must be greater than initial price.");

        RuleFor(x => x.AuctionStartTime)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("Auction start time must be in the future.");

        RuleFor(x => x.AuctionEndTime)
            .GreaterThan(x => x.AuctionStartTime)
            .WithMessage("Auction end time must be after start time.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required.");

        RuleFor(x => x.ImageUrl)
            .NotEmpty().WithMessage("Artwork image is required.");
    }
}