using FluentValidation;

namespace UniHub.Learning.Application.Commands.CourseManagement;

public sealed class CreateCourseCommandValidator : AbstractValidator<CreateCourseCommand>
{
    public CreateCourseCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("Course code is required")
            .MaximumLength(50)
            .WithMessage("Course code must not exceed 50 characters")
            .Matches(@"^[A-Z0-9]+$")
            .WithMessage("Course code must contain only uppercase letters and numbers");

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

        RuleFor(x => x.CreatedBy)
            .NotEmpty()
            .WithMessage("Creator ID is required");

        // FacultyId should not be empty Guid when provided (null is acceptable)
        RuleFor(x => x.FacultyId)
            .Must(id => id == null || id != Guid.Empty)
            .WithMessage("Faculty ID cannot be empty when provided");
    }
}
