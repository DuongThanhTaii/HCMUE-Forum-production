using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Learning.Domain.Courses.ValueObjects;

/// <summary>
/// Course code (e.g., CS101, MATH201, PHY301)
/// Value Object với validation
/// </summary>
public sealed record CourseCode
{
    public const int MaxLength = 20;
    public const int MinLength = 3;

    public string Value { get; }

    /// <summary>Private parameterless constructor for EF Core.</summary>
    private CourseCode() { Value = string.Empty; }

    private CourseCode(string value)
    {
        Value = value;
    }

    public static Result<CourseCode> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<CourseCode>(
                new Error("CourseCode.Empty", "Course code cannot be empty"));
        }

        value = value.Trim().ToUpperInvariant(); // Chuẩn hóa thành chữ hoa

        if (value.Length < MinLength)
        {
            return Result.Failure<CourseCode>(
                new Error("CourseCode.TooShort", $"Course code must be at least {MinLength} characters"));
        }

        if (value.Length > MaxLength)
        {
            return Result.Failure<CourseCode>(
                new Error("CourseCode.TooLong", $"Course code cannot exceed {MaxLength} characters"));
        }

        // Validate format: chỉ chấp nhận chữ cái, số và gạch ngang
        if (!System.Text.RegularExpressions.Regex.IsMatch(value, @"^[A-Z0-9\-]+$"))
        {
            return Result.Failure<CourseCode>(
                new Error("CourseCode.InvalidFormat", 
                    "Course code can only contain uppercase letters, numbers, and hyphens"));
        }

        return Result.Success(new CourseCode(value));
    }

    public override string ToString() => Value;

    public static implicit operator string(CourseCode code) => code.Value;
}
