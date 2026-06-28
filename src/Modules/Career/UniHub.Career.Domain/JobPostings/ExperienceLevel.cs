namespace UniHub.Career.Domain.JobPostings;

/// <summary>
/// Represents the required experience level for a job position.
/// </summary>
public enum ExperienceLevel
{
    /// <summary>No experience required (sinh viên mới ra trường)</summary>
    Entry = 0,

    /// <summary>1-3 years of experience</summary>
    Junior = 1,

    /// <summary>3-5 years of experience</summary>
    Mid = 2,

    /// <summary>5-8 years of experience</summary>
    Senior = 3,

    /// <summary>8+ years, leadership roles</summary>
    Lead = 4,

    /// <summary>Executive / director level</summary>
    Executive = 5
}
