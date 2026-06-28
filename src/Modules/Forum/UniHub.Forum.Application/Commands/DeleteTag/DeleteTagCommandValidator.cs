using FluentValidation;

namespace UniHub.Forum.Application.Commands.DeleteTag;

public sealed class DeleteTagCommandValidator : AbstractValidator<DeleteTagCommand>
{
    public DeleteTagCommandValidator()
    {
        RuleFor(x => x.TagId)
            .GreaterThan(0);
    }
}
