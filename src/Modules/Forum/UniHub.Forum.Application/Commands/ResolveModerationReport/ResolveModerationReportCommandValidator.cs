using FluentValidation;

namespace UniHub.Forum.Application.Commands.ResolveModerationReport;

public sealed class ResolveModerationReportCommandValidator : AbstractValidator<ResolveModerationReportCommand>
{
    public ResolveModerationReportCommandValidator()
    {
        RuleFor(x => x.ReportId)
            .GreaterThan(0);

        RuleFor(x => x.ReviewerId)
            .NotEmpty();

        RuleFor(x => x.Action)
            .NotEmpty()
            .Must(action => action is "keep" or "remove")
            .WithMessage("Action must be either 'keep' or 'remove'.");
    }
}
