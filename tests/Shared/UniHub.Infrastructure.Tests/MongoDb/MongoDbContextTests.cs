using FluentAssertions;
using MongoDB.Driver;
using UniHub.Infrastructure.MongoDb;

namespace UniHub.Infrastructure.Tests.MongoDb;

public class MongoDbContextTests
{
    [Fact]
    public void Constructor_WhenDatabaseIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange, Act & Assert
        var act = () => new MongoDbContext(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("database");
    }

    [Fact]
    public void Constructor_WhenDatabaseProvided_ShouldSetDatabase()
    {
        // Arrange
        var database = CreateInMemoryDatabase();

        // Act
        var context = new MongoDbContext(database);

        // Assert
        context.Database.Should().NotBeNull();
        context.Database.DatabaseNamespace.DatabaseName.Should().Be("test_db");
    }

    [Fact]
    public void GetCollection_ShouldReturnCollection()
    {
        // Arrange
        var database = CreateInMemoryDatabase();
        var context = new MongoDbContext(database);

        // Act
        var collection = context.GetCollection<TestDocument>("test_collection");

        // Assert
        collection.Should().NotBeNull();
        collection.CollectionNamespace.CollectionName.Should().Be("test_collection");
    }

    [Fact]
    public void GetCollection_MultipleCalls_ShouldReturnDifferentCollections()
    {
        // Arrange
        var database = CreateInMemoryDatabase();
        var context = new MongoDbContext(database);

        // Act
        var collection1 = context.GetCollection<TestDocument>("collection1");
        var collection2 = context.GetCollection<TestDocument>("collection2");

        // Assert
        collection1.CollectionNamespace.CollectionName.Should().Be("collection1");
        collection2.CollectionNamespace.CollectionName.Should().Be("collection2");
    }

    private static IMongoDatabase CreateInMemoryDatabase()
    {
        var client = new MongoClient("mongodb://localhost:27017");
        return client.GetDatabase("test_db");
    }

    // Test document for testing purposes
    private class TestDocument
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
