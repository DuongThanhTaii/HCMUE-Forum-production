using UniHub.SharedKernel.Domain;

namespace UniHub.Forum.Domain.Posts;

public sealed record PostId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static PostId CreateUnique() => new(Guid.NewGuid());
    
    public static PostId Create(Guid value) => new(value);
    
    public override string ToString() => Value.ToString();
}
