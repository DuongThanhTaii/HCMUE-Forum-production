using System.Text.Json;
using UniHub.Identity.Domain.Permissions;
using UniHub.Identity.Domain.Users.ValueObjects;

namespace UniHub.Identity.Infrastructure.Persistence.Configurations;

internal static class IdentityModelConversion
{
    public static string ToPermissionScopeDb(PermissionScope scope)
    {
        return $"{(int)scope.Type}|{scope.Value}";
    }

    public static PermissionScope ToPermissionScopeDomain(string raw)
    {
        var parts = raw.Split('|', 2);
        var type = (PermissionScopeType)int.Parse(parts[0]);
        var value = parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1]) ? parts[1] : null;
        return PermissionScope.Create(type, value).Value;
    }

    public static string ToUserProfileDb(UserProfile profile)
    {
        var dto = new UserProfileDto(
            profile.FirstName,
            profile.LastName,
            profile.Avatar,
            profile.Bio,
            profile.Phone,
            profile.DateOfBirth);

        return JsonSerializer.Serialize(dto);
    }

    public static UserProfile ToUserProfileDomain(string raw)
    {
        var dto = JsonSerializer.Deserialize<UserProfileDto>(raw)
            ?? throw new InvalidOperationException("Unable to deserialize user profile");

        return UserProfile.Create(
            dto.FirstName,
            dto.LastName,
            dto.Avatar,
            dto.Bio,
            dto.Phone,
            dto.DateOfBirth).Value;
    }

    private sealed record UserProfileDto(
        string FirstName,
        string LastName,
        string? Avatar,
        string? Bio,
        string? Phone,
        DateTime? DateOfBirth);
}
