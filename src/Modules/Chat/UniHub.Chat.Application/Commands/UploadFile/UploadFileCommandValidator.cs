using FluentValidation;

namespace UniHub.Chat.Application.Commands.UploadFile;

/// <summary>
/// Validator for UploadFileCommand
/// </summary>
public sealed class UploadFileCommandValidator : AbstractValidator<UploadFileCommand>
{
    private const long MaxFileSizeMB = 50; // 50 MB
    private const long MaxFileSizeBytes = MaxFileSizeMB * 1024 * 1024;

    private static readonly string[] AllowedContentTypes = new[]
    {
        // Images
        "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp", "image/svg+xml",
        // Documents
        "application/pdf", "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "application/vnd.ms-powerpoint",
        "application/vnd.openxmlformats-officedocument.presentationml.presentation",
        "text/plain", "text/csv",
        // Archives
        "application/zip", "application/x-rar-compressed", "application/x-7z-compressed",
        // Videos
        "video/mp4", "video/mpeg", "video/quicktime", "video/x-msvideo", "video/webm",
        // Audio
        "audio/mpeg", "audio/mp4", "audio/wav", "audio/webm", "audio/ogg"
    };

    public UploadFileCommandValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty()
            .WithMessage("File name is required")
            .MaximumLength(255)
            .WithMessage("File name cannot exceed 255 characters");

        RuleFor(x => x.FileStream)
            .NotNull()
            .WithMessage("File stream is required");

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .WithMessage("Content type is required")
            .Must(BeAllowedContentType)
            .WithMessage($"Content type must be one of: {string.Join(", ", AllowedContentTypes)}");

        RuleFor(x => x.FileSize)
            .GreaterThan(0)
            .WithMessage("File size must be greater than 0")
            .LessThanOrEqualTo(MaxFileSizeBytes)
            .WithMessage($"File size cannot exceed {MaxFileSizeMB} MB");

        RuleFor(x => x.UploadedBy)
            .NotEmpty()
            .WithMessage("UploadedBy is required");
    }

    private static string NormalizeMime(string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return string.Empty;

        var trimmed = contentType.Trim();
        var semi = trimmed.IndexOf(';', StringComparison.Ordinal);
        return semi >= 0 ? trimmed[..semi].Trim() : trimmed;
    }

    private bool BeAllowedContentType(string contentType)
    {
        var primary = NormalizeMime(contentType);
        return AllowedContentTypes.Contains(primary, StringComparer.OrdinalIgnoreCase);
    }
}
