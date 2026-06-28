using UniHub.SharedKernel.Domain;

namespace UniHub.Career.Domain.Companies;

/// <summary>
/// Strongly-typed identifier for Company aggregate.
/// </summary>
public sealed record CompanyId(Guid Value) : GuidId(Value)
{
    public static CompanyId Create(Guid value) => new(value);
    public static CompanyId CreateUnique() => new(Guid.NewGuid());
}
