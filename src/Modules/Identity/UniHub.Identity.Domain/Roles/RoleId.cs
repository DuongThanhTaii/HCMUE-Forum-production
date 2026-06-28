using UniHub.SharedKernel.Domain;

namespace UniHub.Identity.Domain.Roles;

public sealed record RoleId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static RoleId CreateUnique() => new(Guid.NewGuid());
    
    public static RoleId Create(Guid value) => new(value);
    
    public override string ToString() => Value.ToString();
}