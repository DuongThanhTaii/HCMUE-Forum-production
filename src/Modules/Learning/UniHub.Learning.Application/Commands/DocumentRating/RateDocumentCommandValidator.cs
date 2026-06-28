using FluentValidation;

namespace UniHub.Learning.Application.Commands.DocumentRating;

public sealed class RateDocumentCommandValidator : AbstractValidator<RateDocumentCommand>
{
    public RateDocumentCommandValidator()
    {
        RuleFor(x => x.DocumentId)
            .NotEmpty()
            .WithMessage("Document ID is required");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .WithMessage("Rating must be between 1 and 5");
    }
}
