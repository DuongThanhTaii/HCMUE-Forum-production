using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace UniHub.Identity.Infrastructure.Authentication;

internal sealed class AzureAdJwtBearerOptionsSetup : IConfigureNamedOptions<JwtBearerOptions>
{
    public const string Scheme = "AzureAd";

    private readonly AzureAdOptions _options;

    public AzureAdJwtBearerOptionsSetup(IOptions<AzureAdOptions> options)
    {
        _options = options.Value;
    }

    public void Configure(JwtBearerOptions options)
    {
        Configure(Scheme, options);
    }

    public void Configure(string? name, JwtBearerOptions options)
    {
        if (!string.Equals(name, Scheme, StringComparison.Ordinal))
        {
            return;
        }

        if (!_options.Enabled || string.IsNullOrWhiteSpace(_options.TenantId) || string.IsNullOrWhiteSpace(_options.ClientId))
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = false,
            };
            return;
        }

        var authority = _options.Authority?.Trim();
        if (string.IsNullOrWhiteSpace(authority))
        {
            authority = $"https://login.microsoftonline.com/{_options.TenantId}/v2.0";
        }

        var audience = string.IsNullOrWhiteSpace(_options.Audience)
            ? _options.ClientId
            : _options.Audience;

        var tenantId = _options.TenantId.Trim();
        var issuerSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrWhiteSpace(_options.ValidIssuer))
        {
            issuerSet.Add(_options.ValidIssuer.Trim());
        }

        // v2 endpoint + v1 sts issuer (tokens may use either)
        issuerSet.Add($"https://login.microsoftonline.com/{tenantId}/v2.0");
        issuerSet.Add($"https://sts.windows.net/{tenantId}/");

        options.Authority = authority;
        options.RequireHttpsMetadata = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuers = issuerSet.ToArray(),
            ValidateAudience = true,
            ValidAudiences = [$"api://{_options.ClientId}", audience!, _options.ClientId],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = "name",
            RoleClaimType = "roles",
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // SignalR sends JWT via query string on negotiate / WebSocket upgrade.
                var path = context.HttpContext.Request.Path;
                if (!path.StartsWithSegments("/hubs"))
                {
                    return Task.CompletedTask;
                }

                var accessToken = context.Request.Query["access_token"].FirstOrDefault();
                if (!string.IsNullOrEmpty(accessToken))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            },
        };
    }
}
