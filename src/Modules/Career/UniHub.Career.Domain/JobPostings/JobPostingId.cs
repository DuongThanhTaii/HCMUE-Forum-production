using UniHub.SharedKernel.Domain;

namespace UniHub.Career.Domain.JobPostings;

/// <summary>
/// Strongly-typed identifier for JobPosting aggregate.
/// </summary>
public sealed record JobPostingId(Guid Value) : GuidId(Value)
{
    public static JobPostingId Create(Guid value) => new(value);
    public static JobPostingId CreateUnique() => new(Guid.NewGuid());
}
