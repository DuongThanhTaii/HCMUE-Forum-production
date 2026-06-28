using MongoDB.Driver;

namespace UniHub.Infrastructure.MongoDb;

/// <summary>
/// MongoDB context for accessing MongoDB database and collections.
/// </summary>
public sealed class MongoDbContext
{
    /// <summary>
    /// Gets the MongoDB database instance.
    /// </summary>
    public IMongoDatabase Database { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MongoDbContext"/> class.
    /// </summary>
    /// <param name="database">The MongoDB database instance.</param>
    public MongoDbContext(IMongoDatabase database)
    {
        Database = database ?? throw new ArgumentNullException(nameof(database));
    }

    /// <summary>
    /// Gets a collection from the database.
    /// </summary>
    /// <typeparam name="TDocument">The document type.</typeparam>
    /// <param name="collectionName">The collection name.</param>
    /// <returns>The MongoDB collection.</returns>
    public IMongoCollection<TDocument> GetCollection<TDocument>(string collectionName)
    {
        return Database.GetCollection<TDocument>(collectionName);
    }

    /// <summary>
    /// Creates an index on a collection if it doesn't exist.
    /// </summary>
    /// <typeparam name="TDocument">The document type.</typeparam>
    /// <param name="collectionName">The collection name.</param>
    /// <param name="indexDefinition">The index definition.</param>
    /// <param name="indexOptions">Optional index options.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task CreateIndexAsync<TDocument>(
        string collectionName,
        IndexKeysDefinition<TDocument> indexDefinition,
        CreateIndexOptions? indexOptions = null)
    {
        var collection = GetCollection<TDocument>(collectionName);
        var indexModel = new CreateIndexModel<TDocument>(indexDefinition, indexOptions);
        await collection.Indexes.CreateOneAsync(indexModel);
    }

    /// <summary>
    /// Creates multiple indexes on a collection.
    /// </summary>
    /// <typeparam name="TDocument">The document type.</typeparam>
    /// <param name="collectionName">The collection name.</param>
    /// <param name="indexModels">The index models to create.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task CreateIndexesAsync<TDocument>(
        string collectionName,
        IEnumerable<CreateIndexModel<TDocument>> indexModels)
    {
        var collection = GetCollection<TDocument>(collectionName);
        await collection.Indexes.CreateManyAsync(indexModels);
    }
}
