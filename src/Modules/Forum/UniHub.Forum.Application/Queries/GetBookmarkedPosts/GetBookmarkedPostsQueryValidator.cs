using FluentValidation;

namespace UniHub.Forum.Application.Queries.GetBookmarkedPosts;

public sealed class GetBookmarkedPostsQueryValidator : AbstractValidator<GetBookmarkedPostsQuery>
{
    public GetBookmarkedPostsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.PageNumber)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);
    }
}
