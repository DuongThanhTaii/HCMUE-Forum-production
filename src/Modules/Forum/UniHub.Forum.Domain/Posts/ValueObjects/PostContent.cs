using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Forum.Domain.Posts.ValueObjects;

public sealed class PostContent : ValueObject
{
    public const int MinLength = 10;
    public const int MaxLength = 50000;

    public string Value { get; private set; }

    /// <summary>Private parameterless constructor for EF Core.</summary>
    private PostContent() { Value = string.Empty; }

    private PostContent(string value)
    {
        Value = value;
    }

    public static Result<PostContent> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<PostContent>(new Error(
                "PostContent.Empty", 
                "Post content cannot be empty"));
        }

        var trimmedValue = value.Trim();

        if (trimmedValue.Length < MinLength)
        {
            return Result.Failure<PostContent>(new Error(
                "PostContent.TooShort", 
                $"Post content must be at least {MinLength} characters"));
        }

        if (trimmedValue.Length > MaxLength)
        {
            return Result.Failure<PostContent>(new Error(
                "PostContent.TooLong", 
                $"Post content cannot exceed {MaxLength} characters"));
        }

        return Result.Success(new PostContent(trimmedValue));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
