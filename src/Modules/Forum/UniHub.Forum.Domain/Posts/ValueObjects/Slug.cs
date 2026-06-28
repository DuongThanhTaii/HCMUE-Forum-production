using System.Text;
using System.Text.RegularExpressions;
using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Forum.Domain.Posts.ValueObjects;

public sealed partial class Slug : ValueObject
{
    public const int MaxLength = 250;

    public string Value { get; private set; }

    /// <summary>Private parameterless constructor for EF Core.</summary>
    private Slug() { Value = string.Empty; }

    private Slug(string value)
    {
        Value = value;
    }

    public static Result<Slug> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<Slug>(new Error(
                "Slug.Empty", 
                "Slug cannot be empty"));
        }

        var slug = GenerateSlug(value);

        if (slug.Length > MaxLength)
        {
            return Result.Failure<Slug>(new Error(
                "Slug.TooLong", 
                $"Slug cannot exceed {MaxLength} characters"));
        }

        return Result.Success(new Slug(slug));
    }

    public static Result<Slug> CreateFromTitle(PostTitle title)
    {
        return Create(title.Value);
    }

    private static string GenerateSlug(string input)
    {
        // Convert to lowercase
        var slug = input.ToLowerInvariant();

        // Remove accents
        slug = RemoveAccents(slug);

        // Replace spaces with hyphens
        slug = SpaceRegex().Replace(slug, "-");

        // Remove invalid characters
        slug = InvalidCharsRegex().Replace(slug, "");

        // Remove duplicate hyphens
        slug = MultipleHyphensRegex().Replace(slug, "-");

        // Trim hyphens from ends
        slug = slug.Trim('-');

        return slug;
    }

    private static string RemoveAccents(string text)
    {
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"\s+")]
    private static partial Regex SpaceRegex();

    [GeneratedRegex(@"[^a-z0-9\-]")]
    private static partial Regex InvalidCharsRegex();

    [GeneratedRegex(@"\-{2,}")]
    private static partial Regex MultipleHyphensRegex();
}
