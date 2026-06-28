using UniHub.SharedKernel.CQRS;

namespace UniHub.Career.Application.Commands.Applications.SubmitApplication;

/// <summary>
/// Command to submit a new job application.
/// </summary>
public sealed record SubmitApplicationCommand(
    Guid JobPostingId,
    Guid ApplicantId,
    string ResumeFileName,
    string ResumeFileUrl,
    long ResumeFileSizeBytes,
    string ResumeContentType,
    string? CoverLetterContent = null) : ICommand<ApplicationResponse>;

/// <summary>
/// Response DTO for application operations.
/// </summary>
public sealed record ApplicationResponse(
    Guid Id,
    Guid JobPostingId,
    Guid ApplicantId,
    string Status,
    ResumeDto Resume,
    string? CoverLetter,
    DateTime SubmittedAt,
    DateTime? LastStatusChangedAt,
    Guid? LastStatusChangedBy,
    string? ReviewNotes);

/// <summary>
/// Resume data transfer object.
/// </summary>
public sealed record ResumeDto(
    string FileName,
    string FileUrl,
    long FileSizeBytes,
    string ContentType);
