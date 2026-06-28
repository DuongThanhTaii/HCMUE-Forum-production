using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Learning.Domain.Documents.ValueObjects;

/// <summary>
/// Tiêu đề tài liệu (Value Object)
/// </summary>
public sealed record DocumentTitle
{
    public const int MaxLength = 200;
    public const int MinLength = 5;

    public string Value { get; }

    /// <summary>Private parameterless constructor for EF Core.</summary>
    private DocumentTitle() { Value = string.Empty; }

    private DocumentTitle(string value)
    {
        Value = value;
    }

    public static Result<DocumentTitle> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<DocumentTitle>(
                new Error("DocumentTitle.Empty", "Document title cannot be empty"));
        }

        if (value.Length < MinLength)
        {
            return Result.Failure<DocumentTitle>(
                new Error("DocumentTitle.TooShort", $"Document title must be at least {MinLength} characters"));
        }

        if (value.Length > MaxLength)
        {
            return Result.Failure<DocumentTitle>(
                new Error("DocumentTitle.TooLong", $"Document title cannot exceed {MaxLength} characters"));
        }

        return Result.Success(new DocumentTitle(value.Trim()));
    }

    public override string ToString() => Value;

    public static implicit operator string(DocumentTitle title) => title.Value;
}
