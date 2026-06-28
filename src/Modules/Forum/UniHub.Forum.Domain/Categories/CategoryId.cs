using UniHub.SharedKernel.Domain;

namespace UniHub.Forum.Domain.Categories;

public sealed record CategoryId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static CategoryId CreateUnique() => new(Guid.NewGuid());
    
    public static CategoryId Create(Guid value) => new(value);
    
    public override string ToString() => Value.ToString();
}
