using FluentValidation;

namespace UniHub.Chat.Application.Queries.ListConversationLinks;

public sealed class ListConversationLinksQueryValidator : AbstractValidator<ListConversationLinksQuery>
{
    public ListConversationLinksQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.ConversationId).NotEmpty();
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
