using UniHub.SharedKernel.Domain;

namespace UniHub.Career.Domain.Recruiters;

public sealed record RecruiterId(Guid Value) : GuidId(Value)
{
    public static RecruiterId Create(Guid value) => new(value);
    public static RecruiterId CreateUnique() => new(Guid.NewGuid());
}
