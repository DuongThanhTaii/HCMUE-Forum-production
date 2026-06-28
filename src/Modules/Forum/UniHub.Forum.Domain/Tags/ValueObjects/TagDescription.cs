using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Forum.Domain.Tags.ValueObjects;

public sealed class TagDescription : ValueObject
{
    public const int MaxLength = 500;

    public string Value { get; }

    /// <summary>Private parameterless constructor for EF Core.</summary>
    private TagDescription() { Value = string.Empty; }

    private TagDescription(string value)
    {
        Value = value;
    }

    public static Result<TagDescription> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Success(new TagDescription(string.Empty));
        }

        var trimmed = value.Trim();

        if (trimmed.Length > MaxLength)
        {
            return Result.Failure<TagDescription>(new Error(
                "TagDescription.TooLong",
                $"Tag description cannot exceed {MaxLength} characters"));
        }

        return Result.Success(new TagDescription(trimmed));
    }

    public static implicit operator string(TagDescription description) => description.Value;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
