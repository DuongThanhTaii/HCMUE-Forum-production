using UniHub.SharedKernel.Domain;

namespace UniHub.Career.Domain.Applications;

/// <summary>
/// Strongly-typed identifier for Application entities.
/// </summary>
public sealed record ApplicationId(Guid Value) : GuidId(Value)
{
    /// <summary>
    /// Creates a new ApplicationId from a GUID.
    /// </summary>
    public static ApplicationId Create(Guid value) => new(value);

    /// <summary>
    /// Generates a new unique ApplicationId.
    /// </summary>
    public static ApplicationId CreateUnique() => new(Guid.NewGuid());
}
