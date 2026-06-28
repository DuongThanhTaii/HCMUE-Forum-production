namespace UniHub.Identity.Application.Abstractions;

/// <summary>
/// Ensures a local Forum user exists as a guest in Azure Entra ID.
/// Used for just-in-time invitation when local login succeeds.
/// </summary>
public interface IAzureGuestInvitationService
{
    Task EnsureInvitedAsync(
        Guid userId,
        string email,
        string displayName,
        CancellationToken cancellationToken = default);
}
