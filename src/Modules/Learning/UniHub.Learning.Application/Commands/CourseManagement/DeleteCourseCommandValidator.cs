using FluentValidation;

namespace UniHub.Learning.Application.Commands.CourseManagement;

public sealed class DeleteCourseCommandValidator : AbstractValidator<DeleteCourseCommand>
{
    public DeleteCourseCommandValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty()
            .WithMessage("Course ID is required");

        RuleFor(x => x.DeletedBy)
            .NotEmpty()
            .WithMessage("Deleter ID is required");
    }
}
