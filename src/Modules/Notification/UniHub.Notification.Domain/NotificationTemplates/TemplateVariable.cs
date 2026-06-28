using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Notification.Domain.NotificationTemplates;

/// <summary>
/// Value object representing a template variable placeholder.
/// Variables are used in templates like {UserName}, {JobTitle}, etc.
/// </summary>
public sealed class TemplateVariable : ValueObject
{
    /// <summary>Variable name (without braces, e.g., "UserName").</summary>
    public string Name { get; private set; }

    /// <summary>Human-readable description of the variable.</summary>
    public string Description { get; private set; }

    /// <summary>Example value for documentation purposes (optional).</summary>
    public string? ExampleValue { get; private set; }

    public const int MaxNameLength = 50;
    public const int MaxDescriptionLength = 500;
    public const int MaxExampleLength = 200;

    /// <summary>Private constructor for EF Core.</summary>
    private TemplateVariable()
    {
        Name = string.Empty;
        Description = string.Empty;
    }

    private TemplateVariable(
        string name,
        string description,
        string? exampleValue)
    {
        Name = name;
        Description = description;
        ExampleValue = exampleValue;
    }

    /// <summary>
    /// Creates a new TemplateVariable value object.
    /// </summary>
    public static Result<TemplateVariable> Create(
        string name,
        string description,
        string? exampleValue = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<TemplateVariable>(
                new Error("TemplateVariable.EmptyName", "Variable name is required."));

        if (string.IsNullOrWhiteSpace(description))
            return Result.Failure<TemplateVariable>(
                new Error("TemplateVariable.EmptyDescription", "Variable description is required."));

        // Trim whitespace
        name = name.Trim();
        description = description.Trim();
        exampleValue = exampleValue?.Trim();

        // Validate name format (alphanumeric and underscores only)
        if (!System.Text.RegularExpressions.Regex.IsMatch(name, "^[a-zA-Z0-9_]+$"))
            return Result.Failure<TemplateVariable>(
                new Error("TemplateVariable.InvalidNameFormat",
                    "Variable name must contain only letters, numbers, and underscores."));

        if (name.Length > MaxNameLength)
            return Result.Failure<TemplateVariable>(
                new Error("TemplateVariable.NameTooLong",
                    $"Variable name cannot exceed {MaxNameLength} characters."));

        if (description.Length > MaxDescriptionLength)
            return Result.Failure<TemplateVariable>(
                new Error("TemplateVariable.DescriptionTooLong",
                    $"Variable description cannot exceed {MaxDescriptionLength} characters."));

        if (exampleValue?.Length > MaxExampleLength)
            return Result.Failure<TemplateVariable>(
                new Error("TemplateVariable.ExampleTooLong",
                    $"Variable example cannot exceed {MaxExampleLength} characters."));

        return Result.Success(new TemplateVariable(name, description, exampleValue));
    }

    /// <summary>
    /// Returns the variable in template placeholder format {VariableName}.
    /// </summary>
    public string ToPlaceholder() => $"{{{Name}}}";

    public override string ToString()
        => $"{{{Name}}} - {Description}";

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Name;
        yield return Description;
        yield return ExampleValue;
    }
}
