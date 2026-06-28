using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Identity.Domain.Permissions;

public sealed class Permission : Entity<PermissionId>
{
    public string Code { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public string Module { get; private set; }
    public string Resource { get; private set; }
    public string Action { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Permission()
    {
        // EF Core constructor
        Code = string.Empty;
        Name = string.Empty;
        Module = string.Empty;
        Resource = string.Empty;
        Action = string.Empty;
    }

    private Permission(
        PermissionId id,
        string code,
        string name,
        string module,
        string resource,
        string action,
        string? description = null)
    {
        Id = id;
        Code = code;
        Name = name;
        Module = module;
        Resource = resource;
        Action = action;
        Description = description;
        CreatedAt = DateTime.UtcNow;
    }

    public static Result<Permission> Create(
        string code,
        string name,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Result.Failure<Permission>(new Error("Permission.Code.Empty", "Permission code cannot be empty"));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Permission>(new Error("Permission.Name.Empty", "Permission name cannot be empty"));
        }

        var trimmedCode = code.Trim().ToLowerInvariant();
        var parts = trimmedCode.Split('.');
        
        if (parts.Length != 3)
        {
            return Result.Failure<Permission>(new Error(
                "Permission.Code.InvalidFormat", 
                "Permission code must follow format: {module}.{resource}.{action}"));
        }

        var module = parts[0];
        var resource = parts[1];
        var action = parts[2];

        if (string.IsNullOrWhiteSpace(module) || 
            string.IsNullOrWhiteSpace(resource) || 
            string.IsNullOrWhiteSpace(action))
        {
            return Result.Failure<Permission>(new Error(
                "Permission.Code.InvalidParts", 
                "All parts of permission code (module, resource, action) must be non-empty"));
        }

        if (trimmedCode.Length > 100)
        {
            return Result.Failure<Permission>(new Error("Permission.Code.TooLong", "Permission code cannot exceed 100 characters"));
        }

        if (name.Trim().Length > 200)
        {
            return Result.Failure<Permission>(new Error("Permission.Name.TooLong", "Permission name cannot exceed 200 characters"));
        }

        if (description?.Length > 500)
        {
            return Result.Failure<Permission>(new Error("Permission.Description.TooLong", "Permission description cannot exceed 500 characters"));
        }

        return Result.Success(new Permission(
            PermissionId.CreateUnique(),
            trimmedCode,
            name.Trim(),
            module,
            resource,
            action,
            description?.Trim()));
    }

    public Result UpdateDescription(string? description)
    {
        if (description?.Length > 500)
        {
            return Result.Failure(new Error("Permission.Description.TooLong", "Permission description cannot exceed 500 characters"));
        }

        Description = description?.Trim();
        return Result.Success();
    }

    public bool IsForModule(string moduleName) => 
        string.Equals(Module, moduleName, StringComparison.OrdinalIgnoreCase);

    public bool IsForResource(string resourceName) => 
        string.Equals(Resource, resourceName, StringComparison.OrdinalIgnoreCase);

    public bool IsForAction(string actionName) => 
        string.Equals(Action, actionName, StringComparison.OrdinalIgnoreCase);
}