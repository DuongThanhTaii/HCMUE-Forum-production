using FluentValidation;
using System.Linq;

namespace UniHub.Learning.Application.Commands.UploadDocument;

/// <summary>
/// Validator for UploadDocumentCommand
/// </summary>
public sealed class UploadDocumentCommandValidator : AbstractValidator<UploadDocumentCommand>
{
    private const long MaxFileSizeBytes = 50 * 1024 * 1024; // 50MB
    private static readonly string[] AllowedContentTypes = new[]
    {
        "application/pdf",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document", // .docx
        "application/vnd.openxmlformats-officedocument.presentationml.presentation", // .pptx
        "application/zip",
        "video/mp4",
        "text/plain",
        "image/jpeg",
        "image/png"
    };

    public UploadDocumentCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MinimumLength(5).WithMessage("Title must be at least 5 characters")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters");

        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("File name is required")
            .MaximumLength(255).WithMessage("File name must not exceed 255 characters");

        RuleFor(x => x.FileContent)
            .NotNull().WithMessage("File content is required")
            .NotEmpty().WithMessage("File cannot be empty");

        RuleFor(x => x.FileSize)
            .GreaterThan(0).WithMessage("File size must be greater than 0")
            .LessThanOrEqualTo(MaxFileSizeBytes).WithMessage($"File size must not exceed {MaxFileSizeBytes / 1024 / 1024}MB");

        RuleFor(x => x.ContentType)
            .NotEmpty().WithMessage("Content type is required")
            .Must(ct => AllowedContentTypes.Contains(ct.ToLowerInvariant()))
            .WithMessage($"Content type must be one of: {string.Join(", ", AllowedContentTypes)}");

        RuleFor(x => x.DocumentType)
            .IsInEnum().WithMessage("Invalid document type");

        RuleFor(x => x.UploaderId)
            .NotEmpty().WithMessage("Uploader ID is required");
    }
}
