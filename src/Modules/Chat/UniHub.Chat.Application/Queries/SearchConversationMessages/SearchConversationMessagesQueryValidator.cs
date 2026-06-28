using FluentValidation;

namespace UniHub.Chat.Application.Queries.SearchConversationMessages;

public sealed class SearchConversationMessagesQueryValidator
    : AbstractValidator<SearchConversationMessagesQuery>
{
    private static readonly string[] AllowedFilters = ["all", "text", "media", "links"];

    public SearchConversationMessagesQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.ConversationId).NotEmpty();
        RuleFor(x => x.Q)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(200);
        RuleFor(x => x.Filter)
            .Must(f => AllowedFilters.Contains(f, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Filter must be all, text, media, or links");
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
    }
}
