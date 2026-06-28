using Microsoft.EntityFrameworkCore;
using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Domain.Tokens;
using UniHub.Identity.Domain.Users;
using UniHub.Infrastructure.Persistence;

namespace UniHub.Identity.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of refresh token repository
/// </summary>
public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ApplicationDbContext _context;

    public RefreshTokenRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == token, cancellationToken);
    }

    public async Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null && now < t.ExpiresAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
    }

    public Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        _context.RefreshTokens.Update(refreshToken);
        return Task.CompletedTask;
    }

    public async Task RevokeAllByUserIdAsync(UserId userId, string? revokedByIp = null, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var userTokens = await _context.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null && now < t.ExpiresAt)
            .ToListAsync(cancellationToken);
            
        foreach (var token in userTokens)
        {
            token.Revoke(revokedByIp, "Revoked all tokens");
        }
    }

    public async Task RemoveExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var expiredTokens = await _context.RefreshTokens
            .Where(t => now >= t.ExpiresAt)
            .ToListAsync(cancellationToken);
            
        _context.RefreshTokens.RemoveRange(expiredTokens);
    }
}
