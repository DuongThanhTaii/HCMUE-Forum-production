using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Forum.Domain.Categories.ValueObjects;

public sealed class CategoryDescription : ValueObject
{
    public const int MaxLength = 500;

    public string Value { get; private set; }

    /// <summary>Private parameterless constructor for EF Core.</summary>
    private CategoryDescription() { Value = string.Empty; }

    private CategoryDescription(string value)
    {
        Value = value;
    }

    public static Result<CategoryDescription> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Success(new CategoryDescription(string.Empty));
        }

        var trimmedValue = value.Trim();

        if (trimmedValue.Length > MaxLength)
        {
            return Result.Failure<CategoryDescription>(new Error(
                "CategoryDescription.TooLong", 
                $"Category description cannot exceed {MaxLength} characters"));
        }

        return Result.Success(new CategoryDescription(trimmedValue));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
