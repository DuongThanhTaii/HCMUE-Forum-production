using UniHub.SharedKernel.Domain;

namespace UniHub.Learning.Domain.Courses;

/// <summary>
/// Strongly typed ID cho Course aggregate
/// </summary>
public sealed record CourseId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static CourseId CreateUnique() => new(Guid.NewGuid());
    
    public static CourseId Create(Guid value) => new(value);
    
    public override string ToString() => Value.ToString();
}
