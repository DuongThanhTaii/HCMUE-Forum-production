using UniHub.SharedKernel.Domain;

namespace UniHub.Learning.Domain.Faculties;

/// <summary>
/// Strongly-typed ID cho Faculty aggregate
/// </summary>
public sealed record FacultyId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static FacultyId CreateUnique() => new(Guid.NewGuid());

    public static FacultyId Create(Guid value) => new(value);

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(FacultyId id) => id.Value;
}
