using FluentValidation;

namespace UniHub.Career.Application.Commands.Companies.ApproveCompany;

public sealed class ApproveCompanyCommandValidator : AbstractValidator<ApproveCompanyCommand>
{
    public ApproveCompanyCommandValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty()
            .WithMessage("CompanyId is required");

        RuleFor(x => x.ApprovedBy)
            .NotEmpty()
            .WithMessage("ApprovedBy is required");
    }
}
