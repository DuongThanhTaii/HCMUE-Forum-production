using UniHub.SharedKernel.Domain;

namespace UniHub.Forum.Domain.Comments;

public sealed record CommentId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static CommentId CreateUnique() => new(Guid.NewGuid());
    
    public static CommentId Create(Guid value) => new(value);
    
    public override string ToString() => Value.ToString();
}
