using System.Security.Claims;
using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Domain.Users;
using UniHub.Identity.Domain.Users.ValueObjects;

namespace UniHub.API.Middlewares;

/// <summary>
/// Ensures an Azure-authenticated principal has a corresponding local Identity user.
/// Also rewrites NameIdentifier/user_id claims to the local user ID so downstream modules
/// can continue using local GUID references.
/// </summary>
public sealed class AzureUserProvisioningMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AzureUserProvisioningMiddleware> _logger;

    public AzureUserProvisioningMiddleware(
        RequestDelegate next,
        ILogger<AzureUserProvisioningMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true && IsAzurePrincipal(context.User))
        {
            await EnsureLocalUserAsync(context);
        }

        await _next(context);
    }

    private async Task EnsureLocalUserAsync(HttpContext context)
    {
        var email = GetFirstClaimValue(
            context.User,
            ClaimTypes.Email,
            "email",
            "preferred_username",
            "upn");
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            return;
        }

        using var scope = context.RequestServices.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var roleRepository = scope.ServiceProvider.GetRequiredService<IRoleRepository>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var emailResult = Email.Create(email);
        if (emailResult.IsFailure)
        {
            return;
        }

        var user = await userRepository.GetByEmailAsync(emailResult.Value, context.RequestAborted);
        if (user is null)
        {
            user = await CreateLocalUserFromAzureAsync(
                context.User,
                userRepository,
                roleRepository,
                passwordHasher,
                emailResult.Value,
                context.RequestAborted);
        }

        if (user is null)
        {
            return;
        }

        RewriteLocalIdentityClaims(context.User, user);
    }

    private async Task<User?> CreateLocalUserFromAzureAsync(
        ClaimsPrincipal principal,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher,
        Email email,
        CancellationToken cancellationToken)
    {
        var fullName =
            GetFirstClaimValue(principal, ClaimTypes.Name, "name", "given_name") ??
            email.Value.Split('@')[0];
        var nameParts = fullName
            .Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var firstName = nameParts.Length > 0 ? nameParts[0] : "Azure";
        var lastName = nameParts.Length > 1 ? string.Join(' ', nameParts.Skip(1)) : "User";

        var profileResult = UserProfile.Create(firstName, lastName);
        if (profileResult.IsFailure)
        {
            return null;
        }

        var passwordHash = passwordHasher.HashPassword(Guid.NewGuid().ToString("N"));
        var userResult = User.Create(email, passwordHash, profileResult.Value);
        if (userResult.IsFailure)
        {
            return null;
        }

        var user = userResult.Value;
        var studentRole = await roleRepository.GetByNameAsync("Student", cancellationToken);
        if (studentRole is not null)
        {
            user.AssignRole(studentRole.Id);
        }

        try
        {
            await userRepository.AddAsync(user, cancellationToken);
            _logger.LogInformation(
                "Provisioned local user {UserId} from Azure principal ({Email})",
                user.Id.Value,
                email.Value);
            return user;
        }
        catch
        {
            // Concurrent request can create the same user by email.
            return await userRepository.GetByEmailAsync(email, cancellationToken);
        }
    }

    private static bool IsAzurePrincipal(ClaimsPrincipal principal)
    {
        var issuer = GetFirstClaimValue(principal, "iss");
        if (string.IsNullOrWhiteSpace(issuer))
        {
            return false;
        }

        return issuer.Contains("login.microsoftonline.com", StringComparison.OrdinalIgnoreCase) ||
               issuer.Contains("sts.windows.net", StringComparison.OrdinalIgnoreCase);
    }

    private static string? GetFirstClaimValue(ClaimsPrincipal principal, params string[] claimTypes)
    {
        foreach (var claimType in claimTypes)
        {
            var value = principal.FindFirst(claimType)?.Value;
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return null;
    }

    private static void RewriteLocalIdentityClaims(ClaimsPrincipal principal, User user)
    {
        var identity = principal.Identity as ClaimsIdentity;
        if (identity is null)
        {
            return;
        }

        var localUserId = user.Id.Value.ToString();
        ReplaceClaim(identity, ClaimTypes.NameIdentifier, localUserId);
        ReplaceClaim(identity, "user_id", localUserId);
        ReplaceClaim(identity, ClaimTypes.Email, user.Email.Value);
        ReplaceClaim(identity, ClaimTypes.Name, user.Profile.FullName);
    }

    private static void ReplaceClaim(ClaimsIdentity identity, string claimType, string claimValue)
    {
        var existing = identity.FindFirst(claimType);
        if (existing is not null)
        {
            identity.RemoveClaim(existing);
        }

        identity.AddClaim(new Claim(claimType, claimValue));
    }
}
