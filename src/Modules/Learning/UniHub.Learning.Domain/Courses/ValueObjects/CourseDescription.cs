using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Learning.Domain.Courses.ValueObjects;

/// <summary>
/// Mô tả course (Value Object)
/// </summary>
public sealed record CourseDescription
{
    public const int MaxLength = 2000;

    public string Value { get; }

    /// <summary>Private parameterless constructor for EF Core.</summary>
    private CourseDescription() { Value = string.Empty; }

    private CourseDescription(string value)
    {
        Value = value;
    }

    public static Result<CourseDescription> Create(string? value)
    {
        // Description is optional
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Success(new CourseDescription(string.Empty));
        }

        if (value.Length > MaxLength)
        {
            return Result.Failure<CourseDescription>(
                new Error("CourseDescription.TooLong", 
                    $"Course description cannot exceed {MaxLength} characters"));
        }

        return Result.Success(new CourseDescription(value.Trim()));
    }

    public override string ToString() => Value;

    public static implicit operator string(CourseDescription description) => description.Value;
}
