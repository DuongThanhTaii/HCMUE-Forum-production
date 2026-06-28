namespace UniHub.Career.Domain.Applications;

/// <summary>
/// Represents the current status of a job application.
/// </summary>
public enum ApplicationStatus
{
    /// <summary>Submitted, awaiting initial review</summary>
    Pending = 0,

    /// <summary>Under review by recruiter/hiring manager</summary>
    Reviewing = 1,

    /// <summary>Shortlisted for next stage</summary>
    Shortlisted = 2,

    /// <summary>Interview scheduled or completed</summary>
    Interviewed = 3,

    /// <summary>Job offer extended to candidate</summary>
    Offered = 4,

    /// <summary>Candidate accepted the offer</summary>
    Accepted = 5,

    /// <summary>Application rejected by company</summary>
    Rejected = 6,

    /// <summary>Candidate withdrew application</summary>
    Withdrawn = 7
}
