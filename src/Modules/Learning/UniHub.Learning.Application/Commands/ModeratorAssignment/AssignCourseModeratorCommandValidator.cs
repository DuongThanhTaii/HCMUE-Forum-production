using FluentValidation;

namespace UniHub.Learning.Application.Commands.ModeratorAssignment;

public sealed class AssignCourseModeratorCommandValidator : AbstractValidator<AssignCourseModeratorCommand>
{
    public AssignCourseModeratorCommandValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty()
            .WithMessage("Course ID is required");

        RuleFor(x => x.ModeratorId)
            .NotEmpty()
            .WithMessage("Moderator ID is required");

        RuleFor(x => x.AssignedBy)
            .NotEmpty()
            .WithMessage("Assigned By ID is required");
    }
}
