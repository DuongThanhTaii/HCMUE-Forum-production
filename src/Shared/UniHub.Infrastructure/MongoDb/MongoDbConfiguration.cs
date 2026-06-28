using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;

namespace UniHub.Infrastructure.MongoDb;

/// <summary>
/// Configures MongoDB conventions and serializers.
/// </summary>
public static class MongoDbConfiguration
{
    private static bool _isConfigured;

    /// <summary>
    /// Configures MongoDB conventions and serializers.
    /// </summary>
    public static void Configure()
    {
        if (_isConfigured)
        {
            return;
        }

        // Register conventions
        var conventionPack = new ConventionPack
        {
            new CamelCaseElementNameConvention(),
            new IgnoreExtraElementsConvention(true),
            new EnumRepresentationConvention(BsonType.String)
        };

        ConventionRegistry.Register(
            "UniHub Conventions",
            conventionPack,
            _ => true);

        // Configure DateTime serialization to use UTC
        BsonSerializer.RegisterSerializer(new DateTimeSerializer(DateTimeKind.Utc));

        // Configure Guid serialization
        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

        _isConfigured = true;
    }
}
