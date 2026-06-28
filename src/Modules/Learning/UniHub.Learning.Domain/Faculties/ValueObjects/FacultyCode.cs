using System.Text.RegularExpressions;
using UniHub.SharedKernel.Results;

namespace UniHub.Learning.Domain.Faculties.ValueObjects;

/// <summary>
/// Faculty code value object (e.g., "CNTT", "TOAN", "LY")
/// </summary>
public sealed record FacultyCode
{
    public const int MinLength = 2;
    public const int MaxLength = 20;
    
    // Pattern: uppercase letters, numbers, underscore only
    private static readonly Regex CodePattern = new(@"^[A-Z0-9_]+$", RegexOptions.Compiled);
    
    public string Value { get; }

    /// <summary>Private parameterless constructor for EF Core.</summary>
    private FacultyCode() { Value = string.Empty; }

    private FacultyCode(string value)
    {
        Value = value;
    }

    public static Result<FacultyCode> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<FacultyCode>(
                new Error("FacultyCode.Empty", "Faculty code cannot be empty"));
        }

        var trimmed = value.Trim().ToUpper();

        if (trimmed.Length < MinLength)
        {
            return Result.Failure<FacultyCode>(
                new Error("FacultyCode.TooShort", $"Faculty code must be at least {MinLength} characters"));
        }

        if (trimmed.Length > MaxLength)
        {
            return Result.Failure<FacultyCode>(
                new Error("FacultyCode.TooLong", $"Faculty code must not exceed {MaxLength} characters"));
        }

        if (!CodePattern.IsMatch(trimmed))
        {
            return Result.Failure<FacultyCode>(
                new Error("FacultyCode.InvalidFormat", "Faculty code can only contain uppercase letters, numbers, and underscores"));
        }

        return Result.Success(new FacultyCode(trimmed));
    }

    public override string ToString() => Value;

    public static implicit operator string(FacultyCode code) => code.Value;
}
