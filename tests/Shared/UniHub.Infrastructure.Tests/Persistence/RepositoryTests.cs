using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UniHub.Infrastructure.Persistence;
using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Persistence;

namespace UniHub.Infrastructure.Tests.Persistence;

public class RepositoryTests
{
    private readonly TestDbContext _context;
    private readonly Repository<TestAggregateRoot, int> _repository;

    public RepositoryTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TestDbContext(options);
        _repository = new Repository<TestAggregateRoot, int>(_context);
    }

    [Fact]
    public async Task GetByIdAsync_WhenEntityExists_ShouldReturnEntity()
    {
        // Arrange
        var entity = new TestAggregateRoot(1) { Name = "Test" };
        await _context.TestAggregates.AddAsync(entity);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Test");
    }

    [Fact]
    public async Task GetByIdAsync_WhenEntityDoesNotExist_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllEntities()
    {
        // Arrange
        var entities = new[]
        {
            new TestAggregateRoot(1) { Name = "Test1" },
            new TestAggregateRoot(2) { Name = "Test2" },
            new TestAggregateRoot(3) { Name = "Test3" }
        };
        await _context.TestAggregates.AddRangeAsync(entities);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task FindAsync_WithSpecification_ShouldReturnMatchingEntities()
    {
        // Arrange
        var entities = new[]
        {
            new TestAggregateRoot(1) { Name = "Apple" },
            new TestAggregateRoot(2) { Name = "Banana" },
            new TestAggregateRoot(3) { Name = "Apricot" }
        };
        await _context.TestAggregates.AddRangeAsync(entities);
        await _context.SaveChangesAsync();

        var specification = new NameStartsWithSpecification("A");

        // Act
        var result = await _repository.FindAsync(specification);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(e => e.Name == "Apple");
        result.Should().Contain(e => e.Name == "Apricot");
    }

    [Fact]
    public async Task FirstOrDefaultAsync_WhenEntityExists_ShouldReturnEntity()
    {
        // Arrange
        var entities = new[]
        {
            new TestAggregateRoot(1) { Name = "Apple" },
            new TestAggregateRoot(2) { Name = "Banana" }
        };
        await _context.TestAggregates.AddRangeAsync(entities);
        await _context.SaveChangesAsync();

        var specification = new NameStartsWithSpecification("B");

        // Act
        var result = await _repository.FirstOrDefaultAsync(specification);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Banana");
    }

    [Fact]
    public async Task FirstOrDefaultAsync_WhenEntityDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var specification = new NameStartsWithSpecification("Z");

        // Act
        var result = await _repository.FirstOrDefaultAsync(specification);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AnyAsync_WhenEntitiesExist_ShouldReturnTrue()
    {
        // Arrange
        var entity = new TestAggregateRoot(1) { Name = "Test" };
        await _context.TestAggregates.AddAsync(entity);
        await _context.SaveChangesAsync();

        var specification = new NameStartsWithSpecification("T");

        // Act
        var result = await _repository.AnyAsync(specification);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task AnyAsync_WhenNoEntitiesExist_ShouldReturnFalse()
    {
        // Arrange
        var specification = new NameStartsWithSpecification("Z");

        // Act
        var result = await _repository.AnyAsync(specification);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CountAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        var entities = new[]
        {
            new TestAggregateRoot(1) { Name = "Apple" },
            new TestAggregateRoot(2) { Name = "Apricot" },
            new TestAggregateRoot(3) { Name = "Banana" }
        };
        await _context.TestAggregates.AddRangeAsync(entities);
        await _context.SaveChangesAsync();

        var specification = new NameStartsWithSpecification("A");

        // Act
        var result = await _repository.CountAsync(specification);

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public async Task AddAsync_ShouldAddEntity()
    {
        // Arrange
        var entity = new TestAggregateRoot(1) { Name = "Test" };

        // Act
        await _repository.AddAsync(entity);
        await _context.SaveChangesAsync();

        // Assert
        _context.TestAggregates.Should().ContainSingle();
    }

    [Fact]
    public async Task AddRangeAsync_ShouldAddMultipleEntities()
    {
        // Arrange
        var entities = new[]
        {
            new TestAggregateRoot(1) { Name = "Test1" },
            new TestAggregateRoot(2) { Name = "Test2" }
        };

        // Act
        await _repository.AddRangeAsync(entities);
        await _context.SaveChangesAsync();

        // Assert
        _context.TestAggregates.Should().HaveCount(2);
    }

    [Fact]
    public async Task Update_ShouldUpdateEntity()
    {
        // Arrange
        var entity = new TestAggregateRoot(1) { Name = "Original" };
        await _context.TestAggregates.AddAsync(entity);
        await _context.SaveChangesAsync();

        // Act
        entity.Name = "Updated";
        _repository.Update(entity);
        await _context.SaveChangesAsync();

        // Assert
        var updated = await _context.TestAggregates.FindAsync(1);
        updated!.Name.Should().Be("Updated");
    }

    [Fact]
    public async Task UpdateRange_ShouldUpdateMultipleEntities()
    {
        // Arrange
        var entities = new[]
        {
            new TestAggregateRoot(1) { Name = "Test1" },
            new TestAggregateRoot(2) { Name = "Test2" }
        };
        await _context.TestAggregates.AddRangeAsync(entities);
        await _context.SaveChangesAsync();

        // Act
        entities[0].Name = "Updated1";
        entities[1].Name = "Updated2";
        _repository.UpdateRange(entities);
        await _context.SaveChangesAsync();

        // Assert
        var updated1 = await _context.TestAggregates.FindAsync(1);
        var updated2 = await _context.TestAggregates.FindAsync(2);
        updated1!.Name.Should().Be("Updated1");
        updated2!.Name.Should().Be("Updated2");
    }

    [Fact]
    public async Task Remove_ShouldRemoveEntity()
    {
        // Arrange
        var entity = new TestAggregateRoot(1) { Name = "Test" };
        await _context.TestAggregates.AddAsync(entity);
        await _context.SaveChangesAsync();

        // Act
        _repository.Remove(entity);
        await _context.SaveChangesAsync();

        // Assert
        _context.TestAggregates.Should().BeEmpty();
    }

    [Fact]
    public async Task RemoveRange_ShouldRemoveMultipleEntities()
    {
        // Arrange
        var entities = new[]
        {
            new TestAggregateRoot(1) { Name = "Test1" },
            new TestAggregateRoot(2) { Name = "Test2" }
        };
        await _context.TestAggregates.AddRangeAsync(entities);
        await _context.SaveChangesAsync();

        // Act
        _repository.RemoveRange(entities);
        await _context.SaveChangesAsync();

        // Assert
        _context.TestAggregates.Should().BeEmpty();
    }

    // Test classes
    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

        public DbSet<TestAggregateRoot> TestAggregates { get; set; } = null!;
    }

    public class TestAggregateRoot : AggregateRoot<int>
    {
        public TestAggregateRoot() : base() { }
        
        public TestAggregateRoot(int id) : base(id) { }

        public string Name { get; set; } = string.Empty;
    }

    public class NameStartsWithSpecification : Specification<TestAggregateRoot>
    {
        private readonly string _prefix;

        public NameStartsWithSpecification(string prefix)
        {
            _prefix = prefix;
        }

        public override System.Linq.Expressions.Expression<Func<TestAggregateRoot, bool>> ToExpression()
        {
            return entity => entity.Name.StartsWith(_prefix);
        }
    }
}
