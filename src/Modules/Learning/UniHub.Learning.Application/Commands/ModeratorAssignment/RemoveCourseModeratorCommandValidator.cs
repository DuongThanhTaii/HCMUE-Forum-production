using FluentValidation;

namespace UniHub.Learning.Application.Commands.ModeratorAssignment;

public sealed class RemoveCourseModeratorCommandValidator : AbstractValidator<RemoveCourseModeratorCommand>
{
    public RemoveCourseModeratorCommandValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty()
            .WithMessage("Course ID is required");

        RuleFor(x => x.ModeratorId)
            .NotEmpty()
            .WithMessage("Moderator ID is required");

        RuleFor(x => x.RemovedBy)
            .NotEmpty()
            .WithMessage("Removed By ID is required");
    }
}
