using UniHub.SharedKernel.Domain;

namespace UniHub.Identity.Domain.Tokens;

public sealed record RefreshTokenId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static RefreshTokenId CreateUnique() => new(Guid.NewGuid());
    
    public static RefreshTokenId Create(Guid value) => new(value);
    
    public override string ToString() => Value.ToString();
}