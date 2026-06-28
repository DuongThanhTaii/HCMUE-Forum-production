using UniHub.Identity.Domain.Errors;
using UniHub.Identity.Domain.Events;
using UniHub.Identity.Domain.Roles;
using UniHub.Identity.Domain.Tokens;
using UniHub.Identity.Domain.Users.ValueObjects;
using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Identity.Domain.Users;

public sealed class User : AggregateRoot<UserId>
{
    private readonly List<UserRole> _roles = new();
    private readonly List<RefreshToken> _refreshTokens = new();

    public Email Email { get; private set; }
    public string PasswordHash { get; private set; }
    public UserProfile Profile { get; private set; }
    public UserStatus Status { get; private set; }
    public OfficialBadge? Badge { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public IReadOnlyCollection<UserRole> Roles => _roles.AsReadOnly();
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    private User()
    {
        // EF Core constructor
        Email = null!;
        PasswordHash = string.Empty;
        Profile = null!;
    }

    private User(UserId id, Email email, string passwordHash, UserProfile profile)
    {
        Id = id;
        Email = email;
        PasswordHash = passwordHash;
        Profile = profile;
        Status = UserStatus.Active;
        CreatedAt = DateTime.UtcNow;
    }

    public static Result<User> Create(Email email, string passwordHash, UserProfile profile)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            return Result.Failure<User>(new Error("User.PasswordHash.Empty", "Password hash cannot be empty"));
        }

        var user = new User(
            UserId.CreateUnique(),
            email,
            passwordHash,
            profile);

        user.AddDomainEvent(new UserRegisteredEvent(user.Id, user.Email));
        return Result.Success(user);
    }

    /// <summary>
    /// Creates a user with a pre-determined ID. Intended only for deterministic data seeding.
    /// </summary>
    public static Result<User> CreateWithId(UserId id, Email email, string passwordHash, UserProfile profile)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            return Result.Failure<User>(new Error("User.PasswordHash.Empty", "Password hash cannot be empty"));
        }

        var user = new User(id, email, passwordHash, profile);
        user.AddDomainEvent(new UserRegisteredEvent(user.Id, user.Email));
        return Result.Success(user);
    }

    public Result UpdateProfile(UserProfile newProfile)
    {
        var oldProfile = Profile;
        Profile = newProfile;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserProfileUpdatedEvent(Id, newProfile));
        return Result.Success();
    }

    public Result ChangeStatus(UserStatus newStatus)
    {
        if (Status == newStatus)
        {
            return Result.Success();
        }

        var oldStatus = Status;
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserStatusChangedEvent(Id, oldStatus, newStatus));
        return Result.Success();
    }

    public Result AssignRole(RoleId roleId)
    {
        if (_roles.Any(r => r.RoleId == roleId))
        {
            return Result.Failure(UserErrors.RoleAlreadyAssigned);
        }

        _roles.Add(new UserRole(Id, roleId));
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new RoleAssignedEvent(Id, roleId));
        return Result.Success();
    }

    public Result RemoveRole(RoleId roleId)
    {
        var userRole = _roles.FirstOrDefault(r => r.RoleId == roleId);
        if (userRole is null)
        {
            return Result.Failure(UserErrors.RoleNotAssigned);
        }

        if (_roles.Count == 1)
        {
            return Result.Failure(UserErrors.CannotDeleteLastRole);
        }

        _roles.Remove(userRole);
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result SetOfficialBadge(OfficialBadge badge)
    {
        if (Badge is not null)
        {
            return Result.Failure(UserErrors.BadgeAlreadyAssigned);
        }

        Badge = badge;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new OfficialBadgeAssignedEvent(Id, badge));
        return Result.Success();
    }

    public Result RemoveOfficialBadge()
    {
        Badge = null;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public RefreshToken AddRefreshToken(string token, DateTime expiresAt, string? ipAddress = null)
    {
        var refreshToken = RefreshToken.Create(Id, token, expiresAt, ipAddress);
        _refreshTokens.Add(refreshToken);
        return refreshToken;
    }

    public void RevokeRefreshToken(string token, string? replacedByToken = null)
    {
        var refreshToken = _refreshTokens.FirstOrDefault(rt => rt.Token == token);
        refreshToken?.Revoke(null, null, replacedByToken);
    }

    public void RevokeAllRefreshTokens()
    {
        foreach (var token in _refreshTokens.Where(rt => rt.IsActive))
        {
            token.Revoke();
        }
    }

    public bool CanLogin()
    {
        return Status == UserStatus.Active;
    }

    public Result ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
        {
            return Result.Failure(new Error("User.PasswordHash.Empty", "Password hash cannot be empty"));
        }

        PasswordHash = newPasswordHash;
        UpdatedAt = DateTime.UtcNow;

        // Revoke all refresh tokens for security
        RevokeAllRefreshTokens();

        return Result.Success();
    }
}