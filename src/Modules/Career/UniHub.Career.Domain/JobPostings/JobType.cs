namespace UniHub.Career.Domain.JobPostings;

/// <summary>
/// Represents the type/nature of the job.
/// </summary>
public enum JobType
{
    /// <summary>Full-time position (thường là 8h/ngày)</summary>
    FullTime = 0,

    /// <summary>Part-time position</summary>
    PartTime = 1,

    /// <summary>Internship (thực tập)</summary>
    Internship = 2,

    /// <summary>Freelance / contract work</summary>
    Freelance = 3,

    /// <summary>Remote position</summary>
    Remote = 4,

    /// <summary>Temporary / seasonal</summary>
    Temporary = 5
}
