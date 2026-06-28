namespace UniHub.SharedKernel.Results;

/// <summary>
/// Represents an error with a code and message.
/// </summary>
public record Error(string Code, string Message)
{
    /// <summary>
    /// Represents no error.
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty);

    /// <summary>
    /// Represents a null value error.
    /// </summary>
    public static readonly Error NullValue = new("Error.NullValue", "Null value was provided");
}
