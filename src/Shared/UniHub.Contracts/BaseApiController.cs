using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace UniHub.Contracts;

/// <summary>
/// Base controller for all API controllers providing common functionality
/// such as extracting the current user ID from JWT claims.
/// </summary>
[ApiController]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>
    /// Gets the current user ID from JWT claims (NameIdentifier claim).
    /// </summary>
    /// <returns>The user ID as a Guid.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user ID claim is not found or invalid.</exception>
    protected Guid GetCurrentUserId()
    {
        var userIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
            User.FindFirst("user_id")?.Value ??
            User.FindFirst("oid")?.Value ??
            User.FindFirst("nameid")?.Value ??
            User.FindFirst("sub")?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim))
        {
            throw new UnauthorizedAccessException("User ID claim not found. User must be authenticated.");
        }

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException($"Invalid user ID format: {userIdClaim}");
        }

        return userId;
    }

    /// <summary>
    /// Returns the current user ID when authenticated; otherwise null (for optional per-user fields on anonymous endpoints).
    /// </summary>
    protected Guid? TryGetCurrentUserId()
    {
        var userIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
            User.FindFirst("user_id")?.Value ??
            User.FindFirst("oid")?.Value ??
            User.FindFirst("nameid")?.Value ??
            User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return null;
        }

        return userId;
    }
}
