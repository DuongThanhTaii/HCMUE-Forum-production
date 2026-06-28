using FluentValidation;

namespace UniHub.Career.Application.Commands.Recruiters.AddRecruiter;

internal sealed class AddRecruiterCommandValidator : AbstractValidator<AddRecruiterCommand>
{
    public AddRecruiterCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");

        RuleFor(x => x.CompanyId)
            .NotEmpty()
            .WithMessage("Company ID is required");

        RuleFor(x => x.AddedBy)
            .NotEmpty()
            .WithMessage("AddedBy user ID is required");
    }
}
