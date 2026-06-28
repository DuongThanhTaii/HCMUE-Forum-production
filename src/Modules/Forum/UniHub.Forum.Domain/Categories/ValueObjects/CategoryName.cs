using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Forum.Domain.Categories.ValueObjects;

public sealed class CategoryName : ValueObject
{
    public const int MinLength = 2;
    public const int MaxLength = 100;

    public string Value { get; private set; }

    /// <summary>Private parameterless constructor for EF Core.</summary>
    private CategoryName() { Value = string.Empty; }

    private CategoryName(string value)
    {
        Value = value;
    }

    public static Result<CategoryName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<CategoryName>(new Error(
                "CategoryName.Empty", 
                "Category name cannot be empty"));
        }

        var trimmedValue = value.Trim();

        if (trimmedValue.Length < MinLength)
        {
            return Result.Failure<CategoryName>(new Error(
                "CategoryName.TooShort", 
                $"Category name must be at least {MinLength} characters"));
        }

        if (trimmedValue.Length > MaxLength)
        {
            return Result.Failure<CategoryName>(new Error(
                "CategoryName.TooLong", 
                $"Category name cannot exceed {MaxLength} characters"));
        }

        return Result.Success(new CategoryName(trimmedValue));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
