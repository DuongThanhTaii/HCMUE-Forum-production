using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Forum.Domain.Tags.ValueObjects;

public sealed class TagName : ValueObject
{
    public const int MaxLength = 50;
    public const int MinLength = 2;

    public string Value { get; }

    /// <summary>Private parameterless constructor for EF Core.</summary>
    private TagName() { Value = string.Empty; }

    private TagName(string value)
    {
        Value = value;
    }

    public static Result<TagName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<TagName>(new Error(
                "TagName.Empty",
                "Tag name cannot be empty"));
        }

        var trimmed = value.Trim();

        if (trimmed.Length < MinLength)
        {
            return Result.Failure<TagName>(new Error(
                "TagName.TooShort",
                $"Tag name must be at least {MinLength} characters"));
        }

        if (trimmed.Length > MaxLength)
        {
            return Result.Failure<TagName>(new Error(
                "TagName.TooLong",
                $"Tag name cannot exceed {MaxLength} characters"));
        }

        // Tag names should only contain alphanumeric characters, hyphens, and underscores
        if (!System.Text.RegularExpressions.Regex.IsMatch(trimmed, @"^[a-zA-Z0-9\-_]+$"))
        {
            return Result.Failure<TagName>(new Error(
                "TagName.InvalidFormat",
                "Tag name can only contain letters, numbers, hyphens, and underscores"));
        }

        return Result.Success(new TagName(trimmed));
    }

    public static implicit operator string(TagName name) => name.Value;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
