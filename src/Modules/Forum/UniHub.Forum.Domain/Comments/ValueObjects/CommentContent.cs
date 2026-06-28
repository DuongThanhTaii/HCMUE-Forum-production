using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Forum.Domain.Comments.ValueObjects;

public sealed class CommentContent : ValueObject
{
    public const int MinLength = 1;
    public const int MaxLength = 10000;

    public string Value { get; private set; }

    /// <summary>Private parameterless constructor for EF Core.</summary>
    private CommentContent() { Value = string.Empty; }

    private CommentContent(string value)
    {
        Value = value;
    }

    public static Result<CommentContent> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<CommentContent>(new Error(
                "CommentContent.Empty", 
                "Comment content cannot be empty"));
        }

        var trimmedValue = value.Trim();

        if (trimmedValue.Length < MinLength)
        {
            return Result.Failure<CommentContent>(new Error(
                "CommentContent.TooShort", 
                $"Comment content must be at least {MinLength} character"));
        }

        if (trimmedValue.Length > MaxLength)
        {
            return Result.Failure<CommentContent>(new Error(
                "CommentContent.TooLong", 
                $"Comment content cannot exceed {MaxLength} characters"));
        }

        return Result.Success(new CommentContent(trimmedValue));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
