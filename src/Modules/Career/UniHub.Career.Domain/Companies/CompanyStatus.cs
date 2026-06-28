namespace UniHub.Career.Domain.Companies;

/// <summary>
/// Represents the verification and operational status of a company.
/// </summary>
public enum CompanyStatus
{
    /// <summary>Pending verification - newly registered</summary>
    Pending = 0,

    /// <summary>Verified and active - can post jobs</summary>
    Verified = 1,

    /// <summary>Suspended - cannot post new jobs</summary>
    Suspended = 2,

    /// <summary>Inactive - voluntarily deactivated</summary>
    Inactive = 3
}
