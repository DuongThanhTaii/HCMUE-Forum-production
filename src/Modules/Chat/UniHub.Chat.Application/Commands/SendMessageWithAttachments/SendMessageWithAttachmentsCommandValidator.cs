using FluentValidation;

namespace UniHub.Chat.Application.Commands.SendMessageWithAttachments;

/// <summary>
/// Validator for SendMessageWithAttachmentsCommand
/// </summary>
public sealed class SendMessageWithAttachmentsCommandValidator : AbstractValidator<SendMessageWithAttachmentsCommand>
{
    private const int MaxAttachments = 10;

    public SendMessageWithAttachmentsCommandValidator()
    {
        RuleFor(x => x.ConversationId)
            .NotEmpty()
            .WithMessage("ConversationId is required");

        RuleFor(x => x.SenderId)
            .NotEmpty()
            .WithMessage("SenderId is required");

        RuleFor(x => x.Attachments)
            .NotEmpty()
            .WithMessage("At least one attachment is required")
            .Must(attachments => attachments.Count <= MaxAttachments)
            .WithMessage($"Cannot attach more than {MaxAttachments} files");

        RuleForEach(x => x.Attachments)
            .ChildRules(attachment =>
            {
                attachment.RuleFor(a => a.FileName)
                    .NotEmpty()
                    .WithMessage("File name is required");

                attachment.RuleFor(a => a.FileUrl)
                    .NotEmpty()
                    .WithMessage("File URL is required");

                attachment.RuleFor(a => a.FileSize)
                    .GreaterThan(0)
                    .WithMessage("File size must be greater than 0");

                attachment.RuleFor(a => a.MimeType)
                    .NotEmpty()
                    .WithMessage("MIME type is required");
            });
    }
}
