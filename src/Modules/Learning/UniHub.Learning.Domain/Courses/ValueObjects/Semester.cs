using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Learning.Domain.Courses.ValueObjects;

/// <summary>
/// Semester info (e.g., "2024-2025 HK1", "Spring 2024")
/// Value Object
/// </summary>
public sealed record Semester
{
    public const int MaxLength = 50;
    public const int MinLength = 4;

    public string Value { get; }

    /// <summary>Private parameterless constructor for EF Core.</summary>
    private Semester() { Value = string.Empty; }

    private Semester(string value)
    {
        Value = value;
    }

    public static Result<Semester> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<Semester>(
                new Error("Semester.Empty", "Semester cannot be empty"));
        }

        value = value.Trim();

        if (value.Length < MinLength)
        {
            return Result.Failure<Semester>(
                new Error("Semester.TooShort", $"Semester must be at least {MinLength} characters"));
        }

        if (value.Length > MaxLength)
        {
            return Result.Failure<Semester>(
                new Error("Semester.TooLong", $"Semester cannot exceed {MaxLength} characters"));
        }

        return Result.Success(new Semester(value));
    }

    /// <summary>
    /// Helper để tạo semester theo format "YYYY-YYYY HKx"
    /// </summary>
    public static Result<Semester> CreateFromYearAndTerm(int startYear, int term)
    {
        if (startYear < 2000 || startYear > 2100)
        {
            return Result.Failure<Semester>(
                new Error("Semester.InvalidYear", "Year must be between 2000 and 2100"));
        }

        if (term < 1 || term > 3)
        {
            return Result.Failure<Semester>(
                new Error("Semester.InvalidTerm", "Term must be 1, 2, or 3"));
        }

        var value = $"{startYear}-{startYear + 1} HK{term}";
        return Result.Success(new Semester(value));
    }

    public override string ToString() => Value;

    public static implicit operator string(Semester semester) => semester.Value;
}
