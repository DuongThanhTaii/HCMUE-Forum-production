using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using UniHub.API.Integrations;
using UniHub.Contracts;

namespace UniHub.API.Controllers;

[ApiController]
[Route("api/v1/integrations/uebot")]
[Produces("application/json")]
[Authorize]
[EnableRateLimiting("integrations")]
public sealed class UEBotIntegrationController : BaseApiController
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly UEBotIntegrationOptions _options;
    private readonly ILogger<UEBotIntegrationController> _logger;

    public UEBotIntegrationController(
        IHttpClientFactory httpClientFactory,
        IOptions<UEBotIntegrationOptions> options,
        ILogger<UEBotIntegrationController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    [HttpPost("exchange-token")]
    [ProducesResponseType(typeof(ApiResponse<UEBotExchangeTokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> ExchangeToken(CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var email = User.FindFirstValue(ClaimTypes.Email) ??
                    User.FindFirstValue("email") ??
                    User.FindFirstValue("preferred_username") ??
                    User.FindFirstValue("upn");
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(ApiResponses.Failure("Authenticated user email is required for UEBot exchange."));
        }

        var name = User.FindFirstValue(ClaimTypes.Name) ??
                   User.FindFirstValue("name") ??
                   User.FindFirstValue("fullName");

        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = BuildBaseAddress(_options.BaseUrl);
        client.Timeout = TimeSpan.FromSeconds(10);

        var payload = new UEBotInternalExchangeRequest(
            userId.ToString(),
            email.Trim().ToLowerInvariant(),
            string.IsNullOrWhiteSpace(name) ? null : name.Trim(),
            "forum");

        var request = new HttpRequestMessage(HttpMethod.Post, _options.ExchangePath);
        request.Content = new StringContent(
            JsonSerializer.Serialize(payload, JsonOptions),
            Encoding.UTF8,
            "application/json");

        if (!string.IsNullOrWhiteSpace(_options.SharedSecret))
        {
            request.Headers.TryAddWithoutValidation("X-Integration-Secret", _options.SharedSecret);
        }

        HttpResponseMessage response;
        try
        {
            response = await client.SendAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UEBot token exchange upstream request failed for user {UserId}", userId);
            return StatusCode(StatusCodes.Status502BadGateway, ApiResponses.Failure("Unable to reach UEBot sync-api."));
        }

        var rawBody = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "UEBot token exchange rejected for user {UserId}. Status: {StatusCode}, Body: {Body}",
                userId,
                (int)response.StatusCode,
                rawBody);
            return StatusCode(StatusCodes.Status502BadGateway, ApiResponses.Failure("UEBot token exchange failed."));
        }

        UEBotInternalExchangeResponse? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize<UEBotInternalExchangeResponse>(rawBody, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse UEBot exchange response for user {UserId}", userId);
            return StatusCode(StatusCodes.Status502BadGateway, ApiResponses.Failure("UEBot exchange response is invalid."));
        }

        if (parsed is null || string.IsNullOrWhiteSpace(parsed.Token))
        {
            return StatusCode(StatusCodes.Status502BadGateway, ApiResponses.Failure("UEBot exchange response is empty."));
        }

        var result = new UEBotExchangeTokenResponse(
            parsed.Token,
            parsed.ExpiresAt,
            parsed.User,
            _options.SyncApiBaseUrl);

        return Ok(ApiResponses.Success(result));
    }

    private static Uri BuildBaseAddress(string baseUrl)
    {
        var normalized = string.IsNullOrWhiteSpace(baseUrl) ? "http://localhost:4010" : baseUrl.Trim();
        if (!normalized.EndsWith('/'))
        {
            normalized += "/";
        }

        return new Uri(normalized, UriKind.Absolute);
    }
}

public sealed record UEBotExchangeTokenResponse(
    string SyncAccessToken,
    string? SyncExpiresAt,
    UEBotUserInfo? SyncUser,
    string SyncApiBaseUrl);

public sealed record UEBotInternalExchangeRequest(
    string ExternalUserId,
    string Email,
    string? Name,
    string Source);

public sealed record UEBotInternalExchangeResponse(
    string Token,
    string? ExpiresAt,
    UEBotUserInfo? User);

public sealed record UEBotUserInfo(
    string Id,
    string Email,
    string? Name);
