using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Forum.Domain.Posts.ValueObjects;

public sealed class PostTitle : ValueObject
{
    public const int MinLength = 5;
    public const int MaxLength = 200;

    public string Value { get; private set; }

    /// <summary>Private parameterless constructor for EF Core.</summary>
    private PostTitle() { Value = string.Empty; }

    private PostTitle(string value)
    {
        Value = value;
    }

    public static Result<PostTitle> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<PostTitle>(new Error(
                "PostTitle.Empty", 
                "Post title cannot be empty"));
        }

        var trimmedValue = value.Trim();

        if (trimmedValue.Length < MinLength)
        {
            return Result.Failure<PostTitle>(new Error(
                "PostTitle.TooShort", 
                $"Post title must be at least {MinLength} characters"));
        }

        if (trimmedValue.Length > MaxLength)
        {
            return Result.Failure<PostTitle>(new Error(
                "PostTitle.TooLong", 
                $"Post title cannot exceed {MaxLength} characters"));
        }

        return Result.Success(new PostTitle(trimmedValue));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
