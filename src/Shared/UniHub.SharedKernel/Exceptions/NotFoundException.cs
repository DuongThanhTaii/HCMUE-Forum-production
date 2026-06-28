namespace UniHub.SharedKernel.Exceptions;

/// <summary>
/// Exception for when a requested entity is not found.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException()
        : base()
    {
    }

    public NotFoundException(string message)
        : base(message)
    {
    }

    public NotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public NotFoundException(string entityName, object key)
        : base($"Entity '{entityName}' with key '{key}' was not found.")
    {
        EntityName = entityName;
        Key = key;
    }

    public string? EntityName { get; }
    public object? Key { get; }
}
