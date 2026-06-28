using FluentValidation;

namespace UniHub.Chat.Application.Queries.ListConversationAttachments;

public sealed class ListConversationAttachmentsQueryValidator
    : AbstractValidator<ListConversationAttachmentsQuery>
{
    private static readonly string[] AllowedKinds = ["all", "image", "file", "voice"];

    public ListConversationAttachmentsQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.ConversationId).NotEmpty();
        RuleFor(x => x.Kind)
            .Must(k => AllowedKinds.Contains(k, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Kind must be all, image, file, or voice");
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
