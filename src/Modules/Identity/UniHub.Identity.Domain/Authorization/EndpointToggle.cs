using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Identity.Domain.Authorization;

public sealed class EndpointToggle : Entity<Guid>
{
    public string EndpointKey { get; private set; }
    public bool IsEnabled { get; private set; }
    public string? Reason { get; private set; }
    public string UpdatedBy { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public int Version { get; private set; }

    private EndpointToggle()
    {
        EndpointKey = string.Empty;
        UpdatedBy = string.Empty;
    }

    private EndpointToggle(string endpointKey, bool isEnabled, string updatedBy, string? reason)
    {
        Id = Guid.NewGuid();
        EndpointKey = endpointKey;
        IsEnabled = isEnabled;
        UpdatedBy = updatedBy;
        Reason = reason;
        UpdatedAtUtc = DateTime.UtcNow;
        Version = 1;
    }

    public static Result<EndpointToggle> Create(
        string endpointKey,
        bool isEnabled,
        string updatedBy,
        string? reason = null)
    {
        if (string.IsNullOrWhiteSpace(endpointKey))
        {
            return Result.Failure<EndpointToggle>(new Error(
                "EndpointToggle.EndpointKey.Empty",
                "Endpoint key cannot be empty."));
        }

        if (string.IsNullOrWhiteSpace(updatedBy))
        {
            return Result.Failure<EndpointToggle>(new Error(
                "EndpointToggle.UpdatedBy.Empty",
                "UpdatedBy cannot be empty."));
        }

        if (endpointKey.Trim().Length > 200)
        {
            return Result.Failure<EndpointToggle>(new Error(
                "EndpointToggle.EndpointKey.TooLong",
                "Endpoint key cannot exceed 200 characters."));
        }

        if (reason?.Length > 500)
        {
            return Result.Failure<EndpointToggle>(new Error(
                "EndpointToggle.Reason.TooLong",
                "Reason cannot exceed 500 characters."));
        }

        return Result.Success(new EndpointToggle(
            endpointKey.Trim(),
            isEnabled,
            updatedBy.Trim(),
            reason?.Trim()));
    }

    public Result SetStatus(bool isEnabled, string updatedBy, string? reason = null)
    {
        if (string.IsNullOrWhiteSpace(updatedBy))
        {
            return Result.Failure(new Error(
                "EndpointToggle.UpdatedBy.Empty",
                "UpdatedBy cannot be empty."));
        }

        if (reason?.Length > 500)
        {
            return Result.Failure(new Error(
                "EndpointToggle.Reason.TooLong",
                "Reason cannot exceed 500 characters."));
        }

        IsEnabled = isEnabled;
        UpdatedBy = updatedBy.Trim();
        Reason = reason?.Trim();
        UpdatedAtUtc = DateTime.UtcNow;
        Version++;

        return Result.Success();
    }
}
