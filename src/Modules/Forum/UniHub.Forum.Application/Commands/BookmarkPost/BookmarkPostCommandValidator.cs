using FluentValidation;

namespace UniHub.Forum.Application.Commands.BookmarkPost;

public sealed class BookmarkPostCommandValidator : AbstractValidator<BookmarkPostCommand>
{
    public BookmarkPostCommandValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty();

        RuleFor(x => x.UserId)
            .NotEmpty();
    }
}
