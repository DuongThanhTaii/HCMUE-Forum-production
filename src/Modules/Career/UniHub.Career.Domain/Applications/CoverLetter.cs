using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Career.Domain.Applications;

/// <summary>
/// Value object representing a cover letter for a job application.
/// </summary>
public sealed class CoverLetter : ValueObject
{
    /// <summary>The content of the cover letter.</summary>
    public string Content { get; private set; }

    public const int MaxContentLength = 5000;
    public const int MinContentLength = 50;

    /// <summary>Private constructor for EF Core.</summary>
    private CoverLetter()
    {
        Content = string.Empty;
    }

    private CoverLetter(string content)
    {
        Content = content;
    }

    /// <summary>
    /// Creates a new CoverLetter value object.
    /// </summary>
    public static Result<CoverLetter> Create(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return Result.Failure<CoverLetter>(
                new Error("CoverLetter.Empty", "Cover letter content is required."));

        content = content.Trim();

        if (content.Length < MinContentLength)
            return Result.Failure<CoverLetter>(
                new Error("CoverLetter.TooShort", $"Cover letter must be at least {MinContentLength} characters."));

        if (content.Length > MaxContentLength)
            return Result.Failure<CoverLetter>(
                new Error("CoverLetter.TooLong", $"Cover letter cannot exceed {MaxContentLength} characters."));

        return Result.Success(new CoverLetter(content));
    }

    /// <summary>
    /// Creates an empty/optional cover letter.
    /// </summary>
    public static CoverLetter? CreateOptional(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return null;

        var result = Create(content);
        return result.IsSuccess ? result.Value : null;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Content;
    }
}
