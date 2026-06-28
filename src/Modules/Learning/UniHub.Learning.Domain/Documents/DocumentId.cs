using UniHub.SharedKernel.Domain;

namespace UniHub.Learning.Domain.Documents;

/// <summary>
/// Strongly typed ID cho Document aggregate
/// </summary>
public sealed record DocumentId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static DocumentId CreateUnique() => new(Guid.NewGuid());
    
    public static DocumentId Create(Guid value) => new(value);
    
    public override string ToString() => Value.ToString();
}
