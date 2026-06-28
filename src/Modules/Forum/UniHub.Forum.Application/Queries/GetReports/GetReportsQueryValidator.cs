using FluentValidation;

namespace UniHub.Forum.Application.Queries.GetReports;

public sealed class GetReportsQueryValidator : AbstractValidator<GetReportsQuery>
{
    public GetReportsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("PageNumber must be greater than 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid report status")
            .When(x => x.Status.HasValue);

        RuleFor(x => x.ResolutionDecision)
            .IsInEnum().WithMessage("Invalid report resolution decision")
            .When(x => x.ResolutionDecision.HasValue);
    }
}
