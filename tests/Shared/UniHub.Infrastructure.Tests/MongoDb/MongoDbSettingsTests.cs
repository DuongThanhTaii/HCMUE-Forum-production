using FluentAssertions;
using UniHub.Infrastructure.MongoDb;

namespace UniHub.Infrastructure.Tests.MongoDb;

public class MongoDbSettingsTests
{
    [Fact]
    public void SectionName_ShouldBeMongoDbSettings()
    {
        // Act & Assert
        MongoDbSettings.SectionName.Should().Be("MongoDbSettings");
    }

    [Fact]
    public void DefaultConstructor_ShouldSetDefaultValues()
    {
        // Arrange & Act
        var settings = new MongoDbSettings();

        // Assert
        settings.ConnectionString.Should().BeEmpty();
        settings.DatabaseName.Should().BeEmpty();
        settings.ConnectionTimeoutSeconds.Should().Be(30);
        settings.ServerSelectionTimeoutSeconds.Should().Be(30);
        settings.MaxConnectionPoolSize.Should().Be(100);
        settings.MinConnectionPoolSize.Should().Be(0);
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        // Arrange
        var settings = new MongoDbSettings
        {
            ConnectionString = "mongodb://localhost:27017",
            DatabaseName = "test_db",
            ConnectionTimeoutSeconds = 60,
            ServerSelectionTimeoutSeconds = 45,
            MaxConnectionPoolSize = 200,
            MinConnectionPoolSize = 10
        };

        // Act & Assert
        settings.ConnectionString.Should().Be("mongodb://localhost:27017");
        settings.DatabaseName.Should().Be("test_db");
        settings.ConnectionTimeoutSeconds.Should().Be(60);
        settings.ServerSelectionTimeoutSeconds.Should().Be(45);
        settings.MaxConnectionPoolSize.Should().Be(200);
        settings.MinConnectionPoolSize.Should().Be(10);
    }
}
