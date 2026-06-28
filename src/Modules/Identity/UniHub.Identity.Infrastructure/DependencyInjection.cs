using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Infrastructure.Authentication;
using UniHub.Identity.Infrastructure.Authorization;
using UniHub.Identity.Infrastructure.Caching;
using UniHub.Identity.Infrastructure.Persistence.Repositories;

namespace UniHub.Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAuthentication(configuration);
        services.AddRepositories();
        services.AddServices(configuration);

        return services;
    }

    private static IServiceCollection AddAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure JWT settings
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<AzureAdOptions>(configuration.GetSection(AzureAdOptions.SectionName));

        // Validate JWT settings
        services.AddSingleton<IValidateOptions<JwtSettings>, JwtSettingsValidation>();

        // Register JWT service
        services.AddScoped<IJwtService, JwtService>();
        var azureEnabled = configuration.GetValue<bool>($"{AzureAdOptions.SectionName}:Enabled");

        // Configure JWT Bearer authentication
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "DynamicJwt";
                options.DefaultChallengeScheme = "DynamicJwt";
            })
            .AddPolicyScheme("DynamicJwt", "Dynamic JWT Scheme", policy =>
            {
                policy.ForwardDefaultSelector = context =>
                {
                    // SignalR negotiate/WebSockets often send the JWT via ?access_token=... (no Authorization header).
                    var token = TryGetBearerTokenForSchemeSelection(context);
                    if (string.IsNullOrWhiteSpace(token))
                    {
                        return JwtBearerDefaults.AuthenticationScheme;
                    }

                    try
                    {
                        var handler = new JwtSecurityTokenHandler();
                        var jwt = handler.ReadJwtToken(token);
                        var issuer = jwt.Issuer ?? string.Empty;
                        var isAzureIssuer =
                            issuer.Contains("login.microsoftonline.com", StringComparison.OrdinalIgnoreCase) ||
                            issuer.Contains("sts.windows.net", StringComparison.OrdinalIgnoreCase);
                        return azureEnabled && isAzureIssuer
                            ? AzureAdJwtBearerOptionsSetup.Scheme
                            : JwtBearerDefaults.AuthenticationScheme;
                    }
                    catch
                    {
                        return JwtBearerDefaults.AuthenticationScheme;
                    }
                };
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(AzureAdJwtBearerOptionsSetup.Scheme);

        services.ConfigureOptions<JwtBearerOptionsSetup>();
        services.ConfigureOptions<AzureAdJwtBearerOptionsSetup>();

        return services;
    }

    /// <summary>
    /// Authorization header or SignalR <c>access_token</c> query string (negotiate / WebSockets).
    /// </summary>
    private static string? TryGetBearerTokenForSchemeSelection(HttpContext httpContext)
    {
        var authHeader = httpContext.Request.Headers.Authorization.ToString();
        if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            var t = authHeader["Bearer ".Length..].Trim();
            if (!string.IsNullOrWhiteSpace(t))
            {
                return t;
            }
        }

        var fromQuery = httpContext.Request.Query["access_token"].FirstOrDefault();
        return string.IsNullOrWhiteSpace(fromQuery) ? null : fromQuery.Trim();
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserBlockRepository, UserBlockRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
        services.AddScoped<IUserGroupRepository, UserGroupRepository>();
        services.AddScoped<IUserPermissionOverrideRepository, UserPermissionOverrideRepository>();
        services.AddScoped<IGroupPermissionOverrideRepository, GroupPermissionOverrideRepository>();
        services.AddScoped<IEndpointToggleRepository, EndpointToggleRepository>();
        services.AddScoped<IAuthorizationAuditLogRepository, AuthorizationAuditLogRepository>();

        return services;
    }

    private static IServiceCollection AddServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<PermissionCacheOptions>(
            configuration.GetSection(PermissionCacheOptions.SectionName));

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddHttpClient(nameof(AzureGuestInvitationService));
        services.AddHttpClient(nameof(AzureGuestInvitationService) + "-auth");
        services.AddScoped<IAzureGuestInvitationService, AzureGuestInvitationService>();
        var provider = configuration[$"{PermissionCacheOptions.SectionName}:Provider"];
        var useRedis = string.Equals(provider, "Redis", StringComparison.OrdinalIgnoreCase);

        if (useRedis)
        {
            services.AddSingleton<IPermissionCache, RedisPermissionCache>();
        }
        else
        {
            services.AddSingleton<IPermissionCache, InMemoryPermissionCache>();
        }

        services.AddScoped<IPermissionChecker, PermissionChecker>();

        // Phase 1: Permission-based Authorization (Layer 2)
        // PermissionPolicyProvider dynamically creates policies for "Permission:{code}" on the fly.
        // PermissionAuthorizationHandler resolves IPermissionChecker per-request via IServiceScopeFactory.
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

        return services;
    }
}

/// <summary>
/// Validates JWT settings during application startup
/// </summary>
public sealed class JwtSettingsValidation : IValidateOptions<JwtSettings>
{
    public ValidateOptionsResult Validate(string? name, JwtSettings options)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.SecretKey))
        {
            failures.Add("JWT SecretKey is required");
        }
        else if (options.SecretKey.Length < 32)
        {
            failures.Add("JWT SecretKey must be at least 32 characters long");
        }

        if (string.IsNullOrWhiteSpace(options.Issuer))
        {
            failures.Add("JWT Issuer is required");
        }

        if (string.IsNullOrWhiteSpace(options.Audience))
        {
            failures.Add("JWT Audience is required");
        }

        if (options.AccessTokenExpiryMinutes <= 0)
        {
            failures.Add("JWT AccessTokenExpiryMinutes must be greater than 0");
        }

        if (options.RefreshTokenExpiryDays <= 0)
        {
            failures.Add("JWT RefreshTokenExpiryDays must be greater than 0");
        }

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}