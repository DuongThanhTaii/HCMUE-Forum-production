using UniHub.SharedKernel.Domain;

namespace UniHub.Forum.Domain.Tags;

public sealed record TagId(int Value) : StronglyTypedId<int>(Value)
{
    public static TagId Create(int value) => new(value);
    
    public override string ToString() => Value.ToString();
}
