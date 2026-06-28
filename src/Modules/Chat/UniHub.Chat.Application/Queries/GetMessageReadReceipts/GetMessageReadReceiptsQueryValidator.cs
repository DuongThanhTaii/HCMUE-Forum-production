using FluentValidation;

namespace UniHub.Chat.Application.Queries.GetMessageReadReceipts;

/// <summary>
/// Validator for GetMessageReadReceiptsQuery
/// </summary>
public sealed class GetMessageReadReceiptsQueryValidator : AbstractValidator<GetMessageReadReceiptsQuery>
{
    public GetMessageReadReceiptsQueryValidator()
    {
        RuleFor(x => x.MessageId)
            .NotEmpty()
            .WithMessage("MessageId is required");
    }
}
