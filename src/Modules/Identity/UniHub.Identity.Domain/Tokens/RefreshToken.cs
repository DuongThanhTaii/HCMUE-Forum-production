using UniHub.Identity.Domain.Users;
using UniHub.SharedKernel.Domain;

namespace UniHub.Identity.Domain.Tokens;

public sealed class RefreshToken : Entity<RefreshTokenId>
{
    public string Token { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public DateTime CreatedAt { get; private set; }
    public string? CreatedByIp { get; private set; }
    public string? RevokedBy { get; private set; }
    public string? RevokedByIp { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public bool IsActive => RevokedAt == null && !IsExpired;
    public bool IsRevoked => RevokedAt.HasValue;
    public string? ReplacedByToken { get; private set; }
    public string? RevokeReason { get; private set; }
    public UserId UserId { get; private set; }

    private RefreshToken()
    {
        // EF Core constructor
        Token = string.Empty;
        UserId = null!;
    }

    private RefreshToken(UserId userId, string token, DateTime expiresAt, string? createdByIp = null)
    {
        Id = RefreshTokenId.CreateUnique();
        UserId = userId;
        Token = token;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
        CreatedByIp = createdByIp;
    }

    public static RefreshToken Create(UserId userId, string token, DateTime expiresAt, string? createdByIp = null)
    {
        return new RefreshToken(userId, token, expiresAt, createdByIp);
    }

    public void Revoke(string? revokedByIp = null, string? reason = null, string? replacedByToken = null)
    {
        RevokedAt = DateTime.UtcNow;
        RevokedBy = revokedByIp;
        RevokedByIp = revokedByIp;
        RevokeReason = reason;
        ReplacedByToken = replacedByToken;
    }
}