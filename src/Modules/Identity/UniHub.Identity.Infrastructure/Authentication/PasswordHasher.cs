using BCrypt.Net;
using UniHub.Identity.Application.Abstractions;

namespace UniHub.Identity.Infrastructure.Authentication;

/// <summary>
/// Password hasher implementation using BCrypt
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}
