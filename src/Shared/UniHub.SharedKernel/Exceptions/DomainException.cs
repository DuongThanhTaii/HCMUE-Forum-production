namespace UniHub.SharedKernel.Exceptions;

/// <summary>
/// Exception for domain rule violations.
/// </summary>
public class DomainException : Exception
{
    public DomainException()
        : base()
    {
    }

    public DomainException(string message)
        : base(message)
    {
    }

    public DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public DomainException(string name, string message)
        : base($"Domain rule '{name}' was violated: {message}")
    {
        Name = name;
    }

    public string? Name { get; }
}
