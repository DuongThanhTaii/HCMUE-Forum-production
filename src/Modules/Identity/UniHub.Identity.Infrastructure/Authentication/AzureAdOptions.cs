namespace UniHub.Identity.Infrastructure.Authentication;

public sealed class AzureAdOptions
{
    public const string SectionName = "Authentication:AzureAd";

    public bool Enabled { get; init; }

    public string TenantId { get; init; } = string.Empty;

    public string ClientId { get; init; } = string.Empty;

    public string ClientSecret { get; init; } = string.Empty;

    public string? Audience { get; init; }

    public string? Authority { get; init; }

    public string? ValidIssuer { get; init; }

    public bool EnableGuestInvitationOnLocalLogin { get; init; }

    public string? InvitationRedirectUrl { get; init; }

    public string? GraphBaseUrl { get; init; }

    public bool InvitationSendMail { get; init; } = true;
}
