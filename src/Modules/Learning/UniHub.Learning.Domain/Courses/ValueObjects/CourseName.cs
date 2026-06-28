using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Learning.Domain.Courses.ValueObjects;

/// <summary>
/// Tên course (Value Object)
/// </summary>
public sealed record CourseName
{
    public const int MaxLength = 200;
    public const int MinLength = 3;

    public string Value { get; }

    /// <summary>Private parameterless constructor for EF Core.</summary>
    private CourseName() { Value = string.Empty; }

    private CourseName(string value)
    {
        Value = value;
    }

    public static Result<CourseName> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<CourseName>(
                new Error("CourseName.Empty", "Course name cannot be empty"));
        }

        if (value.Length < MinLength)
        {
            return Result.Failure<CourseName>(
                new Error("CourseName.TooShort", $"Course name must be at least {MinLength} characters"));
        }

        if (value.Length > MaxLength)
        {
            return Result.Failure<CourseName>(
                new Error("CourseName.TooLong", $"Course name cannot exceed {MaxLength} characters"));
        }

        return Result.Success(new CourseName(value.Trim()));
    }

    public override string ToString() => Value;

    public static implicit operator string(CourseName name) => name.Value;
}
