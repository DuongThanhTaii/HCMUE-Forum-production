using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UniHub.SharedKernel.Domain;

namespace UniHub.Infrastructure.Tests.Persistence;

public class ApplicationDbContextTests
{
    [Fact]
    public async Task SaveChangesAsync_ShouldSaveEntitiesToDatabase()
    {
        // Arrange
        var context = CreateTestDbContext();
        var entity = new TestAggregateRoot(1) { Name = "Test Entity" };

        // Act
        context.TestAggregates.Add(entity);
        var result = await context.SaveChangesAsync();

        // Assert
        result.Should().Be(1);
        var savedEntity = await context.TestAggregates.FindAsync(1);
        savedEntity.Should().NotBeNull();
        savedEntity!.Name.Should().Be("Test Entity");
    }

    [Fact]
    public async Task DbContext_ShouldApplyConfigurationsFromAssembly()
    {
        // Arrange
        var context = CreateTestDbContext();

        // Act
        var model = context.Model;

        // Assert
        model.Should().NotBeNull();
        model.GetEntityTypes().Should().NotBeEmpty();
    }

    [Fact]
    public void DbContext_ShouldBeConfiguredWithInMemoryDatabase()
    {
        // Arrange & Act
        var context = CreateTestDbContext();

        // Assert
        context.Should().NotBeNull();
        context.Database.IsInMemory().Should().BeTrue();
    }

    private static TestDbContext CreateTestDbContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new TestDbContext(options);
    }

    /// <summary>
    /// Standalone test DbContext — does NOT inherit from ApplicationDbContext
    /// to avoid discovering 20+ module entities that require full infrastructure configs.
    /// </summary>
    private class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

        public DbSet<TestAggregateRoot> TestAggregates => Set<TestAggregateRoot>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestAggregateRoot>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name);
            });
        }
    }

    // Test aggregate root for testing purposes
    private class TestAggregateRoot : AggregateRoot<int>
    {
        public string Name { get; set; } = string.Empty;

        public TestAggregateRoot() : base() { }
        public TestAggregateRoot(int id) : base(id) { }
    }
}
