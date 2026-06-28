using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Career.Domain.JobPostings;

/// <summary>
/// Value object representing the work location for a job posting.
/// </summary>
public sealed class WorkLocation : ValueObject
{
    /// <summary>City name (e.g., "Hồ Chí Minh", "Hà Nội").</summary>
    public string City { get; private set; }

    /// <summary>District or area within the city (optional).</summary>
    public string? District { get; private set; }

    /// <summary>Full street address (optional).</summary>
    public string? Address { get; private set; }

    /// <summary>Whether the position allows remote work.</summary>
    public bool IsRemote { get; private set; }

    public const int MaxCityLength = 100;
    public const int MaxDistrictLength = 100;
    public const int MaxAddressLength = 300;

    /// <summary>Private constructor for EF Core.</summary>
    private WorkLocation()
    {
        City = string.Empty;
    }

    private WorkLocation(string city, string? district, string? address, bool isRemote)
    {
        City = city;
        District = district;
        Address = address;
        IsRemote = isRemote;
    }

    /// <summary>
    /// Creates a new WorkLocation value object.
    /// </summary>
    public static Result<WorkLocation> Create(
        string city,
        string? district = null,
        string? address = null,
        bool isRemote = false)
    {
        if (string.IsNullOrWhiteSpace(city))
            return Result.Failure<WorkLocation>(
                new Error("WorkLocation.EmptyCity", "City is required."));

        if (city.Length > MaxCityLength)
            return Result.Failure<WorkLocation>(
                new Error("WorkLocation.CityTooLong", $"City cannot exceed {MaxCityLength} characters."));

        if (district?.Length > MaxDistrictLength)
            return Result.Failure<WorkLocation>(
                new Error("WorkLocation.DistrictTooLong", $"District cannot exceed {MaxDistrictLength} characters."));

        if (address?.Length > MaxAddressLength)
            return Result.Failure<WorkLocation>(
                new Error("WorkLocation.AddressTooLong", $"Address cannot exceed {MaxAddressLength} characters."));

        return Result.Success(new WorkLocation(city.Trim(), district?.Trim(), address?.Trim(), isRemote));
    }

    /// <summary>
    /// Creates a remote-only work location.
    /// </summary>
    public static WorkLocation Remote() => new("Remote", null, null, true);

    /// <summary>
    /// Returns formatted location string.
    /// </summary>
    public override string ToString()
    {
        if (IsRemote && City == "Remote")
            return "Remote";

        var location = District != null ? $"{District}, {City}" : City;
        return IsRemote ? $"{location} (Remote)" : location;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return City;
        yield return District;
        yield return Address;
        yield return IsRemote;
    }
}
