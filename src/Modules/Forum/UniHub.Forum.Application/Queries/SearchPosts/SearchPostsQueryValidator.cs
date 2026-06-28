using FluentValidation;

namespace UniHub.Forum.Application.Queries.SearchPosts;

/// <summary>
/// Validator for SearchPostsQuery
/// </summary>
public sealed class SearchPostsQueryValidator : AbstractValidator<SearchPostsQuery>
{
    public SearchPostsQueryValidator()
    {
        RuleFor(x => x.SearchTerm)
            .NotEmpty()
            .WithMessage("Search term is required")
            .MinimumLength(2)
            .WithMessage("Search term must be at least 2 characters")
            .MaximumLength(200)
            .WithMessage("Search term cannot exceed 200 characters");

        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100");

        When(x => x.PostType.HasValue, () =>
        {
            RuleFor(x => x.PostType!.Value)
                .IsInEnum()
                .WithMessage("Invalid post type");
        });

        When(x => x.Tags is not null, () =>
        {
            RuleFor(x => x.Tags!)
                .Must(tags => tags.Count() <= 10)
                .WithMessage("Cannot filter by more than 10 tags");
        });
    }
}
