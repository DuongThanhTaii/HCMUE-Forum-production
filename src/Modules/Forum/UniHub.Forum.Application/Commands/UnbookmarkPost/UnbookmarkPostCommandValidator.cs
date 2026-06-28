using FluentValidation;

namespace UniHub.Forum.Application.Commands.UnbookmarkPost;

public sealed class UnbookmarkPostCommandValidator : AbstractValidator<UnbookmarkPostCommand>
{
    public UnbookmarkPostCommandValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty();

        RuleFor(x => x.UserId)
            .NotEmpty();
    }
}
