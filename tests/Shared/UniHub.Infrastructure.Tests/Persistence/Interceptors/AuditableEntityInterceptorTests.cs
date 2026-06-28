using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using UniHub.Infrastructure.Persistence.Interceptors;
using UniHub.SharedKernel.Domain;

namespace UniHub.Infrastructure.Tests.Persistence.Interceptors;

public class AuditableEntityInterceptorTests
{
    [Fact]
    public async Task SaveChangesAsync_WhenEntityAdded_ShouldSetCreatedAt()
    {
        // Arrange
        var (context, entity) = CreateContextAndEntity();
        context.Set<TestAuditableEntity>().Add(entity);
        var beforeSave = DateTime.UtcNow;

        // Act
        await context.SaveChangesAsync();

        // Assert
        entity.CreatedAt.Should().BeOnOrAfter(beforeSave);
        entity.CreatedAt.Should().BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public async Task SaveChangesAsync_WhenEntityModified_ShouldSetUpdatedAt()
    {
        // Arrange
        var (context, entity) = CreateContextAndEntity();
        context.Set<TestAuditableEntity>().Add(entity);
        await context.SaveChangesAsync();

        var originalCreatedAt = entity.CreatedAt;
        await Task.Delay(10); // Ensure time difference

        // Act
        entity.Name = "Updated Name";
        context.Set<TestAuditableEntity>().Update(entity);
        await context.SaveChangesAsync();

        // Assert
        entity.CreatedAt.Should().Be(originalCreatedAt); // Should not change
        entity.UpdatedAt.Should().NotBeNull();
        entity.UpdatedAt.Should().BeAfter(originalCreatedAt);
    }

    [Fact]
    public async Task SaveChangesAsync_WhenEntityAdded_ShouldNotSetUpdatedAt()
    {
        // Arrange
        var (context, entity) = CreateContextAndEntity();
        context.Set<TestAuditableEntity>().Add(entity);

        // Act
        await context.SaveChangesAsync();

        // Assert
        entity.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public async Task SaveChangesAsync_WhenMultipleEntitiesAdded_ShouldSetAllCreatedAt()
    {
        // Arrange
        var context = CreateTestDbContext();
        var entity1 = new TestAuditableEntity(1) { Name = "Entity 1" };
        var entity2 = new TestAuditableEntity(2) { Name = "Entity 2" };
        context.Set<TestAuditableEntity>().AddRange(entity1, entity2);

        // Act
        await context.SaveChangesAsync();

        // Assert
        entity1.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        entity2.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task SaveChangesAsync_WhenNonAuditableEntityAdded_ShouldNotThrow()
    {
        // Arrange
        var context = CreateTestDbContext();
        var entity = new TestNonAuditableEntity(1) { Name = "Non-Auditable" };
        context.Set<TestNonAuditableEntity>().Add(entity);

        // Act
        var act = async () => await context.SaveChangesAsync();

        // Assert
        await act.Should().NotThrowAsync();
    }

    private static (TestDbContext context, TestAuditableEntity entity) CreateContextAndEntity()
    {
        var context = CreateTestDbContext();
        var entity = new TestAuditableEntity(1) { Name = "Test Entity" };
        return (context, entity);
    }

    private static TestDbContext CreateTestDbContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .AddInterceptors(new AuditableEntityInterceptor())
            .Options;

        return new TestDbContext(options);
    }

    // Test DbContext
    private class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

        public DbSet<TestAuditableEntity> AuditableEntities => Set<TestAuditableEntity>();
        public DbSet<TestNonAuditableEntity> NonAuditableEntities => Set<TestNonAuditableEntity>();
    }

    // Test entities
    private class TestAuditableEntity : Entity<int>, IAuditableEntity
    {
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        public TestAuditableEntity() : base() { }
        public TestAuditableEntity(int id) : base(id) { }
    }

    private class TestNonAuditableEntity : Entity<int>
    {
        public string Name { get; set; } = string.Empty;

        public TestNonAuditableEntity() : base() { }
        public TestNonAuditableEntity(int id) : base(id) { }
    }
}
