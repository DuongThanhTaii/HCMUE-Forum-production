using Microsoft.EntityFrameworkCore;
using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Domain.Users;
using UniHub.Infrastructure.Persistence;

namespace UniHub.Identity.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of password reset token repository
/// </summary>
public sealed class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly ApplicationDbContext _context;

    public PasswordResetTokenRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PasswordResetToken?> GetValidTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var resetToken = await _context.PasswordResetTokens
            .FirstOrDefaultAsync(t => t.Token == token, cancellationToken);
            
        return resetToken?.IsValid() == true ? resetToken : null;
    }

    public async Task AddAsync(PasswordResetToken resetToken, CancellationToken cancellationToken = default)
    {
        await _context.PasswordResetTokens.AddAsync(resetToken, cancellationToken);
    }

    public Task UpdateAsync(PasswordResetToken resetToken, CancellationToken cancellationToken = default)
    {
        _context.PasswordResetTokens.Update(resetToken);
        return Task.CompletedTask;
    }

    public async Task InvalidateUserTokensAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        var userTokens = await _context.PasswordResetTokens
            .Where(t => t.UserId == userId)
            .ToListAsync(cancellationToken);
            
        foreach (var token in userTokens.Where(t => t.IsValid()))
        {
            token.MarkAsUsed();
        }
    }
}
