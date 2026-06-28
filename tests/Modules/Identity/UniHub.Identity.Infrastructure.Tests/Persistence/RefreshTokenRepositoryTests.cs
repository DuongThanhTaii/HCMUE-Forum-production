using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UniHub.Identity.Domain.Tokens;
using UniHub.Identity.Domain.Users;
using UniHub.Identity.Infrastructure.Persistence.Repositories;
using UniHub.Infrastructure.Persistence;
using System.Reflection;

namespace UniHub.Identity.Infrastructure.Tests.Persistence;

public sealed class RefreshTokenRepositoryTests
{
    private readonly RefreshTokenRepository _repository;
    private readonly ApplicationDbContext _context;

    public RefreshTokenRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new IdentityTestDbContext(options);
        _repository = new RefreshTokenRepository(_context);
    }

    /// <summary>
    /// Test DbContext that only applies Identity module configurations.
    /// Non-Identity entities discovered from base class DbSet properties are
    /// ignored via reflection so InMemory provider doesn't fail on unmapped types.
    /// </summary>
    private sealed class IdentityTestDbContext : ApplicationDbContext
    {
        public IdentityTestDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Apply Identity module configurations (converters, keys, relationships)
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(UniHub.Identity.Infrastructure.DependencyInjection).Assembly);

            // Ignore entity types from other modules discovered via base class DbSet properties.
            // Use reflection to call the generic ModelBuilder.Ignore<T>() method.
            var ignoreMethod = typeof(ModelBuilder).GetMethods()
                .First(m => m.Name == "Ignore" && m.IsGenericMethod);

            var entitiesToIgnore = modelBuilder.Model.GetEntityTypes()
                .Where(e =>
                {
                    var ns = e.ClrType.Namespace;
                    return ns != null
                        && !ns.StartsWith("UniHub.Identity")
                        && !ns.StartsWith("UniHub.SharedKernel");
                })
                .Select(e => e.ClrType)
                .ToList();

            foreach (var clrType in entitiesToIgnore)
            {
                ignoreMethod.MakeGenericMethod(clrType).Invoke(modelBuilder, null);
            }
        }
    }

    [Fact]
    public async Task AddAsync_ShouldAddRefreshToken()
    {
        // Arrange
        var userId = UserId.CreateUnique();
        var token = "test-token-12345";
        var expiresAt = DateTime.UtcNow.AddDays(7);
        var refreshToken = RefreshToken.Create(userId, token, expiresAt);

        // Act
        await _repository.AddAsync(refreshToken);
        await _context.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByTokenAsync(token);
        retrieved.Should().NotBeNull();
        retrieved!.Token.Should().Be(token);
        retrieved.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task GetByTokenAsync_WithNonExistentToken_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByTokenAsync("non-existent-token");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveTokensByUserIdAsync_ShouldReturnOnlyActiveTokens()
    {
        // Arrange
        var userId = UserId.CreateUnique();
        
        var activeToken1 = RefreshToken.Create(userId, "active-token-1", DateTime.UtcNow.AddDays(7));
        var activeToken2 = RefreshToken.Create(userId, "active-token-2", DateTime.UtcNow.AddDays(7));
        var revokedToken = RefreshToken.Create(userId, "revoked-token", DateTime.UtcNow.AddDays(7));
        revokedToken.Revoke();
        var expiredToken = RefreshToken.Create(userId, "expired-token", DateTime.UtcNow.AddSeconds(-1));

        await _repository.AddAsync(activeToken1);
        await _repository.AddAsync(activeToken2);
        await _repository.AddAsync(revokedToken);
        await _repository.AddAsync(expiredToken);
        await _context.SaveChangesAsync();

        // Act
        var activeTokens = await _repository.GetActiveTokensByUserIdAsync(userId);

        // Assert
        activeTokens.Should().HaveCount(2);
        activeTokens.Should().Contain(t => t.Token == "active-token-1");
        activeTokens.Should().Contain(t => t.Token == "active-token-2");
        activeTokens.Should().NotContain(t => t.Token == "revoked-token");
        activeTokens.Should().NotContain(t => t.Token == "expired-token");
    }

    [Fact]
    public async Task GetActiveTokensByUserIdAsync_WithDifferentUser_ShouldReturnEmpty()
    {
        // Arrange
        var userId1 = UserId.CreateUnique();
        var userId2 = UserId.CreateUnique();
        
        var token = RefreshToken.Create(userId1, "user1-token", DateTime.UtcNow.AddDays(7));
        await _repository.AddAsync(token);
        await _context.SaveChangesAsync();

        // Act
        var activeTokens = await _repository.GetActiveTokensByUserIdAsync(userId2);

        // Assert
        activeTokens.Should().BeEmpty();
    }

    [Fact]
    public async Task RevokeAllByUserIdAsync_ShouldRevokeAllUserTokens()
    {
        // Arrange
        var userId = UserId.CreateUnique();
        var ipAddress = "192.168.1.100";
        
        var token1 = RefreshToken.Create(userId, "token-1", DateTime.UtcNow.AddDays(7));
        var token2 = RefreshToken.Create(userId, "token-2", DateTime.UtcNow.AddDays(7));
        
        await _repository.AddAsync(token1);
        await _repository.AddAsync(token2);
        await _context.SaveChangesAsync();

        // Act
        await _repository.RevokeAllByUserIdAsync(userId, ipAddress);
        await _context.SaveChangesAsync();

        // Assert
        var activeTokens = await _repository.GetActiveTokensByUserIdAsync(userId);
        activeTokens.Should().BeEmpty();
        
        var retrievedToken1 = await _repository.GetByTokenAsync("token-1");
        retrievedToken1!.IsRevoked.Should().BeTrue();
        retrievedToken1.RevokedByIp.Should().Be(ipAddress);
        
        var retrievedToken2 = await _repository.GetByTokenAsync("token-2");
        retrievedToken2!.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public async Task RemoveExpiredTokensAsync_ShouldRemoveOnlyExpiredTokens()
    {
        // Arrange
        var userId = UserId.CreateUnique();
        
        var activeToken = RefreshToken.Create(userId, "active-token", DateTime.UtcNow.AddDays(7));
        var expiredToken1 = RefreshToken.Create(userId, "expired-token-1", DateTime.UtcNow.AddSeconds(-10));
        var expiredToken2 = RefreshToken.Create(userId, "expired-token-2", DateTime.UtcNow.AddSeconds(-5));
        
        await _repository.AddAsync(activeToken);
        await _repository.AddAsync(expiredToken1);
        await _repository.AddAsync(expiredToken2);
        await _context.SaveChangesAsync();

        // Act
        await _repository.RemoveExpiredTokensAsync();
        await _context.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByTokenAsync("active-token");
        retrieved.Should().NotBeNull();
        
        var expiredRetrieved1 = await _repository.GetByTokenAsync("expired-token-1");
        expiredRetrieved1.Should().BeNull();
        
        var expiredRetrieved2 = await _repository.GetByTokenAsync("expired-token-2");
        expiredRetrieved2.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldNotThrow()
    {
        // Arrange
        var userId = UserId.CreateUnique();
        var token = RefreshToken.Create(userId, "test-token", DateTime.UtcNow.AddDays(7));
        await _repository.AddAsync(token);

        // Act & Assert
        var action = async () => await _repository.UpdateAsync(token);
        await action.Should().NotThrowAsync();
    }
}
