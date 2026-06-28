using UniHub.SharedKernel.Results;

namespace UniHub.Learning.Domain.Faculties.ValueObjects;

/// <summary>
/// Faculty name value object
/// </summary>
public sealed record FacultyName
{
    public const int MinLength = 3;
    public const int MaxLength = 200;
    
    public string Value { get; }

    /// <summary>Private parameterless constructor for EF Core.</summary>
    private FacultyName() { Value = string.Empty; }

    private FacultyName(string value)
    {
        Value = value;
    }

    public static Result<FacultyName> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<FacultyName>(
                new Error("FacultyName.Empty", "Faculty name cannot be empty"));
        }

        var trimmed = value.Trim();

        if (trimmed.Length < MinLength)
        {
            return Result.Failure<FacultyName>(
                new Error("FacultyName.TooShort", $"Faculty name must be at least {MinLength} characters"));
        }

        if (trimmed.Length > MaxLength)
        {
            return Result.Failure<FacultyName>(
                new Error("FacultyName.TooLong", $"Faculty name must not exceed {MaxLength} characters"));
        }

        return Result.Success(new FacultyName(trimmed));
    }

    public override string ToString() => Value;

    public static implicit operator string(FacultyName name) => name.Value;
}
