using FluentValidation;

namespace UniHub.Forum.Application.Queries.GetPopularTags;

public sealed class GetPopularTagsQueryValidator : AbstractValidator<GetPopularTagsQuery>
{
    public GetPopularTagsQueryValidator()
    {
        RuleFor(x => x.Count)
            .InclusiveBetween(1, 50);
    }
}
