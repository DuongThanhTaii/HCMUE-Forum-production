using UniHub.SharedKernel.Domain;

namespace UniHub.Identity.Domain.Users;

public sealed class PasswordResetToken : Entity<Guid>
{
    public UserId UserId { get; private set; }
    public string Token { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsUsed { get; private set; }
    public DateTime? UsedAt { get; private set; }

    private PasswordResetToken()
    {
        // EF Core constructor
        UserId = null!;
        Token = string.Empty;
    }

    private PasswordResetToken(UserId userId, string token, DateTime expiresAt)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Token = token;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
        IsUsed = false;
    }

    public static PasswordResetToken Create(UserId userId, string token, TimeSpan validFor)
    {
        return new PasswordResetToken(userId, token, DateTime.UtcNow.Add(validFor));
    }

    public bool IsValid()
    {
        return !IsUsed && DateTime.UtcNow < ExpiresAt;
    }

    public void MarkAsUsed()
    {
        IsUsed = true;
        UsedAt = DateTime.UtcNow;
    }
}
