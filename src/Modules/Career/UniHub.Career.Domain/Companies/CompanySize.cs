namespace UniHub.Career.Domain.Companies;

/// <summary>
/// Represents the size/scale of a company by employee count.
/// </summary>
public enum CompanySize
{
    /// <summary>Startup - 1-10 employees</summary>
    Startup = 0,

    /// <summary>Small - 11-50 employees</summary>
    Small = 1,

    /// <summary>Medium - 51-200 employees</summary>
    Medium = 2,

    /// <summary>Large - 201-1000 employees</summary>
    Large = 3,

    /// <summary>Enterprise - 1000+ employees</summary>
    Enterprise = 4
}
