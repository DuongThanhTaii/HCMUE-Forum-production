using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Notification.Domain.NotificationTemplates;

/// <summary>
/// Value object representing email notification template content.
/// Supports variable placeholders like {UserName}, {ActionUrl}, etc.
/// </summary>
public sealed class EmailTemplateContent : ValueObject
{
    /// <summary>Email subject line with placeholder support.</summary>
    public string Subject { get; private set; }

    /// <summary>Email body in HTML or plain text with placeholder support.</summary>
    public string Body { get; private set; }

    /// <summary>Sender display name (e.g., "UniHub Team").</summary>
    public string? FromName { get; private set; }

    /// <summary>Sender email address (if null, uses system default).</summary>
    public string? FromEmail { get; private set; }

    public const int MaxSubjectLength = 200;
    public const int MaxBodyLength = 50000;
    public const int MaxFromNameLength = 100;
    public const int MaxFromEmailLength = 256;

    /// <summary>Private constructor for EF Core.</summary>
    private EmailTemplateContent()
    {
        Subject = string.Empty;
        Body = string.Empty;
    }

    private EmailTemplateContent(
        string subject,
        string body,
        string? fromName,
        string? fromEmail)
    {
        Subject = subject;
        Body = body;
        FromName = fromName;
        FromEmail = fromEmail;
    }

    /// <summary>
    /// Creates a new EmailTemplateContent value object.
    /// </summary>
    public static Result<EmailTemplateContent> Create(
        string subject,
        string body,
        string? fromName = null,
        string? fromEmail = null)
    {
        if (string.IsNullOrWhiteSpace(subject))
            return Result.Failure<EmailTemplateContent>(
                new Error("EmailTemplateContent.EmptySubject", "Email subject is required."));

        if (string.IsNullOrWhiteSpace(body))
            return Result.Failure<EmailTemplateContent>(
                new Error("EmailTemplateContent.EmptyBody", "Email body is required."));

        // Trim whitespace
        subject = subject.Trim();
        body = body.Trim();
        fromName = fromName?.Trim();
        fromEmail = fromEmail?.Trim();

        if (subject.Length > MaxSubjectLength)
            return Result.Failure<EmailTemplateContent>(
                new Error("EmailTemplateContent.SubjectTooLong",
                    $"Email subject cannot exceed {MaxSubjectLength} characters."));

        if (body.Length > MaxBodyLength)
            return Result.Failure<EmailTemplateContent>(
                new Error("EmailTemplateContent.BodyTooLong",
                    $"Email body cannot exceed {MaxBodyLength} characters."));

        if (fromName?.Length > MaxFromNameLength)
            return Result.Failure<EmailTemplateContent>(
                new Error("EmailTemplateContent.FromNameTooLong",
                    $"From name cannot exceed {MaxFromNameLength} characters."));

        if (fromEmail?.Length > MaxFromEmailLength)
            return Result.Failure<EmailTemplateContent>(
                new Error("EmailTemplateContent.FromEmailTooLong",
                    $"From email cannot exceed {MaxFromEmailLength} characters."));

        return Result.Success(new EmailTemplateContent(subject, body, fromName, fromEmail));
    }

    public override string ToString()
        => $"Email: {Subject}";

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Subject;
        yield return Body;
        yield return FromName;
        yield return FromEmail;
    }
}
