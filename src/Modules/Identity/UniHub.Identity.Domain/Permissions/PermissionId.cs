using UniHub.SharedKernel.Domain;

namespace UniHub.Identity.Domain.Permissions;

public sealed record PermissionId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static PermissionId CreateUnique() => new(Guid.NewGuid());
    
    public static PermissionId Create(Guid value) => new(value);
    
    public override string ToString() => Value.ToString();
}