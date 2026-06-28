using FluentValidation;

namespace UniHub.Learning.Application.Queries.DocumentSearch;

public sealed class SearchDocumentsQueryValidator : AbstractValidator<SearchDocumentsQuery>
{
    public SearchDocumentsQueryValidator()
    {
        RuleFor(x => x.SearchTerm)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.SearchTerm))
            .WithMessage("Search term cannot exceed 200 characters");

        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100");

        RuleFor(x => x.DocumentType)
            .InclusiveBetween(0, 5)
            .When(x => x.DocumentType.HasValue)
            .WithMessage("Document type must be a valid value (0-5)");

        RuleFor(x => x.Status)
            .InclusiveBetween(0, 4)
            .When(x => x.Status.HasValue)
            .WithMessage("Status must be a valid value (0-4)");

        RuleFor(x => x.FacultyId)
            .Must(id => !id.HasValue || id.Value != Guid.Empty)
            .WithMessage("Faculty id cannot be empty.");

        RuleFor(x => x.CourseId)
            .Must(id => !id.HasValue || id.Value != Guid.Empty)
            .WithMessage("Course id cannot be empty.");
    }
}
