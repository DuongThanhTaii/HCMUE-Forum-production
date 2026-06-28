using FluentValidation;

namespace UniHub.Learning.Application.Commands.DocumentDownload;

public sealed class DownloadDocumentCommandValidator : AbstractValidator<DownloadDocumentCommand>
{
    public DownloadDocumentCommandValidator()
    {
        RuleFor(x => x.DocumentId)
            .NotEmpty()
            .WithMessage("Document ID is required");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");
    }
}
