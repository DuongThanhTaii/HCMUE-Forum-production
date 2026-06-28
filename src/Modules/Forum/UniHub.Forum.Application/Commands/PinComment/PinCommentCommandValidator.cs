using FluentValidation;

namespace UniHub.Forum.Application.Commands.PinComment;

public sealed class PinCommentCommandValidator : AbstractValidator<PinCommentCommand>
{
    public PinCommentCommandValidator()
    {
        RuleFor(x => x.CommentId).NotEmpty();
        RuleFor(x => x.PostId).NotEmpty();
        RuleFor(x => x.RequestingUserId).NotEmpty();
    }
}
