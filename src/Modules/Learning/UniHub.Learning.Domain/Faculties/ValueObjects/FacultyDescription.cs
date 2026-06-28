using UniHub.SharedKernel.Results;

namespace UniHub.Learning.Domain.Faculties.ValueObjects;

/// <summary>
/// Faculty description value object (optional)
/// </summary>
public sealed record FacultyDescription
{
    public const int MaxLength = 2000;
    
    public string Value { get; }

    /// <summary>Private parameterless constructor for EF Core.</summary>
    private FacultyDescription() { Value = string.Empty; }

    private FacultyDescription(string value)
    {
        Value = value ?? string.Empty;
    }

    public static Result<FacultyDescription> Create(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return Result.Success(new FacultyDescription(string.Empty));
        }

        var trimmed = value.Trim();

        if (trimmed.Length > MaxLength)
        {
            return Result.Failure<FacultyDescription>(
                new Error("FacultyDescription.TooLong", $"Faculty description must not exceed {MaxLength} characters"));
        }

        return Result.Success(new FacultyDescription(trimmed));
    }

    public override string ToString() => Value;

    public static implicit operator string(FacultyDescription description) => description.Value;
}
