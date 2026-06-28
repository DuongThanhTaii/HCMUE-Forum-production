using FluentValidation;

namespace UniHub.Career.Application.Commands.Applications.SubmitApplication;

/// <summary>
/// Validator for SubmitApplicationCommand.
/// </summary>
public sealed class SubmitApplicationCommandValidator : AbstractValidator<SubmitApplicationCommand>
{
    private const long MaxResumeSize = 10 * 1024 * 1024; // 10 MB
    private static readonly string[] AllowedContentTypes = { "application/pdf", "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" };

    public SubmitApplicationCommandValidator()
    {
        RuleFor(x => x.JobPostingId)
            .NotEmpty()
            .WithMessage("Job posting ID is required.");

        RuleFor(x => x.ApplicantId)
            .NotEmpty()
            .WithMessage("Applicant ID is required.");

        RuleFor(x => x.ResumeFileName)
            .NotEmpty()
            .WithMessage("Resume file name is required.")
            .MaximumLength(255)
            .WithMessage("Resume file name must not exceed 255 characters.");

        RuleFor(x => x.ResumeFileUrl)
            .NotEmpty()
            .WithMessage("Resume file URL is required.")
            .MaximumLength(2048)
            .WithMessage("Resume file URL must not exceed 2048 characters.")
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("Resume file URL must be a valid URL.");

        RuleFor(x => x.ResumeFileSizeBytes)
            .GreaterThan(0)
            .WithMessage("Resume file size must be greater than 0.")
            .LessThanOrEqualTo(MaxResumeSize)
            .WithMessage($"Resume file size must not exceed {MaxResumeSize / (1024 * 1024)} MB.");

        RuleFor(x => x.ResumeContentType)
            .NotEmpty()
            .WithMessage("Resume content type is required.")
            .Must(ct => AllowedContentTypes.Contains(ct.ToLowerInvariant()))
            .WithMessage("Resume must be in PDF, DOC, or DOCX format.");

        RuleFor(x => x.CoverLetterContent)
            .MinimumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.CoverLetterContent))
            .WithMessage("Cover letter must be at least 50 characters.")
            .MaximumLength(5000)
            .When(x => !string.IsNullOrWhiteSpace(x.CoverLetterContent))
            .WithMessage("Cover letter must not exceed 5000 characters.");
    }
}
