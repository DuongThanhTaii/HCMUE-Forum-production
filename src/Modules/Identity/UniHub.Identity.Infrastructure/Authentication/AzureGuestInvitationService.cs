using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UniHub.Identity.Application.Abstractions;

namespace UniHub.Identity.Infrastructure.Authentication;

internal sealed class AzureGuestInvitationService : IAzureGuestInvitationService
{
    private const string DefaultGraphBaseUrl = "https://graph.microsoft.com/";
    private const string GraphScope = "https://graph.microsoft.com/.default";
    private const string DefaultInvitationRedirectUrl = "http://localhost:5173";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptions<AzureAdOptions> _options;
    private readonly ILogger<AzureGuestInvitationService> _logger;

    public AzureGuestInvitationService(
        IHttpClientFactory httpClientFactory,
        IOptions<AzureAdOptions> options,
        ILogger<AzureGuestInvitationService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options;
        _logger = logger;
    }

    public async Task EnsureInvitedAsync(
        Guid userId,
        string email,
        string displayName,
        CancellationToken cancellationToken = default)
    {
        var options = _options.Value;
        if (!options.Enabled || !options.EnableGuestInvitationOnLocalLogin)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(options.TenantId) ||
            string.IsNullOrWhiteSpace(options.ClientId) ||
            string.IsNullOrWhiteSpace(options.ClientSecret))
        {
            _logger.LogWarning(
                "Azure guest invitation is enabled but missing TenantId/ClientId/ClientSecret.");
            return;
        }

        var accessToken = await AcquireGraphAccessTokenAsync(options, cancellationToken);
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return;
        }

        using var graphClient = _httpClientFactory.CreateClient(nameof(AzureGuestInvitationService));
        graphClient.BaseAddress = new Uri(
            string.IsNullOrWhiteSpace(options.GraphBaseUrl)
                ? DefaultGraphBaseUrl
                : options.GraphBaseUrl,
            UriKind.Absolute);
        graphClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        if (await UserAlreadyExistsAsync(graphClient, email, cancellationToken))
        {
            _logger.LogDebug("Azure user already exists for {Email}. Skip invitation.", email);
            return;
        }

        await InviteGuestAsync(graphClient, options, userId, email, displayName, cancellationToken);
    }

    private async Task<string?> AcquireGraphAccessTokenAsync(AzureAdOptions options, CancellationToken cancellationToken)
    {
        using var authClient = _httpClientFactory.CreateClient(nameof(AzureGuestInvitationService) + "-auth");
        var tokenUrl = $"https://login.microsoftonline.com/{options.TenantId}/oauth2/v2.0/token";
        var form = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("client_id", options.ClientId),
            new KeyValuePair<string, string>("client_secret", options.ClientSecret),
            new KeyValuePair<string, string>("scope", GraphScope),
        ]);

        using var response = await authClient.PostAsync(tokenUrl, form, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Cannot acquire Graph token for Azure guest invitation. Status={StatusCode}, Body={Body}",
                (int)response.StatusCode,
                TrimBody(body));
            return null;
        }

        using var doc = JsonDocument.Parse(body);
        if (!doc.RootElement.TryGetProperty("access_token", out var tokenElement))
        {
            _logger.LogWarning("Graph token response does not contain access_token.");
            return null;
        }

        return tokenElement.GetString();
    }

    private async Task<bool> UserAlreadyExistsAsync(HttpClient graphClient, string email, CancellationToken cancellationToken)
    {
        var safeEmail = email.Replace("'", "''", StringComparison.Ordinal);
        var filter = Uri.EscapeDataString($"mail eq '{safeEmail}' or userPrincipalName eq '{safeEmail}'");
        var requestUri = $"v1.0/users?$top=1&$select=id,mail,userPrincipalName&$filter={filter}";

        using var response = await graphClient.GetAsync(requestUri, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Cannot query existing Azure user for email {Email}. Status={StatusCode}, Body={Body}",
                email,
                (int)response.StatusCode,
                TrimBody(body));
            return false;
        }

        using var doc = JsonDocument.Parse(body);
        if (!doc.RootElement.TryGetProperty("value", out var value) || value.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        return value.GetArrayLength() > 0;
    }

    private async Task InviteGuestAsync(
        HttpClient graphClient,
        AzureAdOptions options,
        Guid userId,
        string email,
        string displayName,
        CancellationToken cancellationToken)
    {
        var redirectUrl = string.IsNullOrWhiteSpace(options.InvitationRedirectUrl)
            ? DefaultInvitationRedirectUrl
            : options.InvitationRedirectUrl;

        var payload = new
        {
            invitedUserEmailAddress = email,
            invitedUserDisplayName = string.IsNullOrWhiteSpace(displayName) ? email : displayName,
            inviteRedirectUrl = redirectUrl,
            sendInvitationMessage = options.InvitationSendMail,
            invitedUserType = "Guest",
        };

        var json = JsonSerializer.Serialize(payload);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await graphClient.PostAsync("v1.0/invitations", content, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation(
                "Azure guest invitation created for user {UserId} ({Email}).",
                userId,
                email);
            return;
        }

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            _logger.LogInformation("Azure guest already exists for {Email} (409).", email);
            return;
        }

        _logger.LogWarning(
            "Failed to create Azure guest invitation for {Email}. Status={StatusCode}, Body={Body}",
            email,
            (int)response.StatusCode,
            TrimBody(body));
    }

    private static string TrimBody(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value.Length <= 1000 ? value : value[..1000];
    }
}
