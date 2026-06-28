using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Learning.Domain.Documents.ValueObjects;

/// <summary>
/// Mô tả tài liệu (Value Object)
/// </summary>
public sealed record DocumentDescription
{
    public const int MaxLength = 1000;

    public string Value { get; }

    /// <summary>Private parameterless constructor for EF Core.</summary>
    private DocumentDescription() { Value = string.Empty; }

    private DocumentDescription(string value)
    {
        Value = value;
    }

    public static Result<DocumentDescription> Create(string? value)
    {
        // Description is optional
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Success(new DocumentDescription(string.Empty));
        }

        if (value.Length > MaxLength)
        {
            return Result.Failure<DocumentDescription>(
                new Error("DocumentDescription.TooLong", $"Document description cannot exceed {MaxLength} characters"));
        }

        return Result.Success(new DocumentDescription(value.Trim()));
    }

    public override string ToString() => Value;

    public static implicit operator string(DocumentDescription description) => description.Value;
}
