using FluentValidation;

namespace UniHub.Learning.Application.Commands.CourseManagement;

public sealed class UpdateCourseCommandValidator : AbstractValidator<UpdateCourseCommand>
{
    public UpdateCourseCommandValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty()
            .WithMessage("Course ID is required");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Course name is required")
            .Length(3, 200)
            .WithMessage("Course name must be between 3 and 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .WithMessage("Course description must not exceed 2000 characters");

        RuleFor(x => x.Semester)
            .NotEmpty()
            .WithMessage("Semester is required")
            .MaximumLength(20)
            .WithMessage("Semester must not exceed 20 characters");

        RuleFor(x => x.Credits)
            .InclusiveBetween(1, 10)
            .WithMessage("Credits must be between 1 and 10");
    }
}
