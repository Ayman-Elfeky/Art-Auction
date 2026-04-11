using FluentValidation;

namespace ArtAuction.Application.Features.Artworks.Commands.CreateArtwork;

public class CreateArtworkCommandValidator : AbstractValidator<CreateArtworkCommand>
{
    public CreateArtworkCommandValidator()
    {
        RuleFor(x => x.Dto.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Dto.Description)
            .NotEmpty().WithMessage("Description is required");

        RuleFor(x => x.Dto.InitialPrice)
            .GreaterThan(0).WithMessage("Initial price must be greater than 0");

        RuleFor(x => x.Dto.AuctionStartTime)
            .NotEmpty().WithMessage("Auction start time is required");

        RuleFor(x => x.Dto.AuctionEndTime)
            .GreaterThan(x => x.Dto.AuctionStartTime)
            .WithMessage("Auction end time must be after start time");

        RuleFor(x => x.Dto.CategoryId)
            .NotEmpty().WithMessage("Category is required");

        RuleFor(x => x.Dto.ImageUrl)
            .NotEmpty().WithMessage("Image URL is required");
    }
}
