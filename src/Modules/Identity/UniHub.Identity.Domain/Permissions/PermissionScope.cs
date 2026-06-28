using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Identity.Domain.Permissions;

public sealed class PermissionScope : ValueObject
{
    public PermissionScopeType Type { get; private set; }
    public string? Value { get; private set; }

    /// <summary>Private parameterless constructor for EF Core.</summary>
    private PermissionScope() { }

    private PermissionScope(PermissionScopeType type, string? value = null)
    {
        Type = type;
        Value = value;
    }

    public static Result<PermissionScope> Create(PermissionScopeType type, string? value = null)
    {
        if (type != PermissionScopeType.Global && string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<PermissionScope>(new Error(
                "PermissionScope.ValueRequired", 
                $"Value is required for scope type {type}"));
        }

        if (type == PermissionScopeType.Global && !string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<PermissionScope>(new Error(
                "PermissionScope.ValueNotAllowed", 
                "Value should not be provided for Global scope type"));
        }

        if (!string.IsNullOrWhiteSpace(value) && value.Length > 100)
        {
            return Result.Failure<PermissionScope>(new Error(
                "PermissionScope.ValueTooLong", 
                "Scope value cannot exceed 100 characters"));
        }

        return Result.Success(new PermissionScope(type, value?.Trim()));
    }

    public static PermissionScope Global() => new(PermissionScopeType.Global);

    public static Result<PermissionScope> Module(string moduleId) => Create(PermissionScopeType.Module, moduleId);
    
    public static Result<PermissionScope> Course(string courseId) => Create(PermissionScopeType.Course, courseId);
    
    public static Result<PermissionScope> Department(string departmentId) => Create(PermissionScopeType.Department, departmentId);
    
    public static Result<PermissionScope> Category(string categoryId) => Create(PermissionScopeType.Category, categoryId);

    public bool IsGlobal => Type == PermissionScopeType.Global;

    public bool MatchesScope(PermissionScope other)
    {
        if (IsGlobal || other.IsGlobal) return true;
        return Type == other.Type && Value == other.Value;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Type;
        yield return Value ?? string.Empty;
    }

    public override string ToString()
    {
        return Type switch
        {
            PermissionScopeType.Global => "Global",
            PermissionScopeType.Module => $"Module:{Value}",
            PermissionScopeType.Course => $"Course:{Value}",
            PermissionScopeType.Department => $"Department:{Value}",
            PermissionScopeType.Category => $"Category:{Value}",
            _ => $"{Type}:{Value}"
        };
    }
}

public enum PermissionScopeType
{
    Global = 1,
    Module = 2,
    Course = 3,
    Department = 4,
    Category = 5
}