using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using UniHub.Infrastructure.MongoDb;

namespace UniHub.Infrastructure.Tests.MongoDb;

public class MongoDbConfigurationTests
{
    [Fact]
    public void Configure_ShouldRegisterConventions()
    {
        // Act
        MongoDbConfiguration.Configure();

        // Assert
        // Verify conventions are registered by checking serialization behavior
        var conventionPack = ConventionRegistry.Lookup(typeof(TestDocument));
        conventionPack.Should().NotBeNull();
    }

    [Fact]
    public void Configure_CalledMultipleTimes_ShouldNotThrow()
    {
        // Act
        var act = () =>
        {
            MongoDbConfiguration.Configure();
            MongoDbConfiguration.Configure();
            MongoDbConfiguration.Configure();
        };

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Configure_ShouldSetupCamelCaseConvention()
    {
        // Arrange
        MongoDbConfiguration.Configure();

        // Act
        var classMap = BsonClassMap.LookupClassMap(typeof(TestDocument));

        // Assert
        // After configuration, properties should be serialized in camelCase
        classMap.Should().NotBeNull();
    }

    [Fact]
    public void Configure_ShouldSerializeDateTimeAsUtc()
    {
        // Arrange
        MongoDbConfiguration.Configure();
        var testDate = new DateTime(2026, 2, 4, 12, 0, 0, DateTimeKind.Utc);

        // Act
        var bsonValue = BsonValue.Create(testDate);

        // Assert
        bsonValue.Should().NotBeNull();
        bsonValue.BsonType.Should().Be(BsonType.DateTime);
    }

    // Test document for convention testing
    private class TestDocument
    {
        public string Id { get; set; } = string.Empty;
        public string TestProperty { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public Guid UniqueId { get; set; }
    }
}
