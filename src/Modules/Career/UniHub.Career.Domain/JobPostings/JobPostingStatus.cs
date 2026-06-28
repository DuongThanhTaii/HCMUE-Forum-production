namespace UniHub.Career.Domain.JobPostings;

/// <summary>
/// Represents the lifecycle status of a job posting.
/// </summary>
public enum JobPostingStatus
{
    /// <summary>Draft - not yet published</summary>
    Draft = 0,

    /// <summary>Published and accepting applications</summary>
    Published = 1,

    /// <summary>Temporarily paused - not accepting new applications</summary>
    Paused = 2,

    /// <summary>Closed - no longer accepting applications</summary>
    Closed = 3,

    /// <summary>Expired - deadline has passed</summary>
    Expired = 4
}
