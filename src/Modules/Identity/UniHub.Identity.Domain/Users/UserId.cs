using UniHub.SharedKernel.Domain;

namespace UniHub.Identity.Domain.Users;

public sealed record UserId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static UserId CreateUnique() => new(Guid.NewGuid());
    
    public static UserId Create(Guid value) => new(value);
    
    public override string ToString() => Value.ToString();
}