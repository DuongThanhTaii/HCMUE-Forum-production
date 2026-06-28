using FluentValidation;

namespace UniHub.Forum.Application.Queries.GetPostById;

/// <summary>
/// Validator for GetPostByIdQuery
/// </summary>
public sealed class GetPostByIdQueryValidator : AbstractValidator<GetPostByIdQuery>
{
    public GetPostByIdQueryValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty()
            .WithMessage("Post ID is required");
    }
}
