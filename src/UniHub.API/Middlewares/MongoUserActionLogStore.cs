using MongoDB.Driver;
using Microsoft.Extensions.Options;
using UniHub.API.Observability;

namespace UniHub.API.Middlewares;

public sealed class MongoUserActionLogStore : IUserActionLogStore
{
    private readonly UserActionLoggingOptions _options;
    private readonly ILogger<MongoUserActionLogStore> _logger;
    private readonly IMongoCollection<UserActionLogEntry> _collection;

    public MongoUserActionLogStore(
        IMongoDatabase database,
        IOptions<UserActionLoggingOptions> options,
        ILogger<MongoUserActionLogStore> logger)
    {
        _options = options.Value;
        _logger = logger;

        var collectionName = string.IsNullOrWhiteSpace(_options.MongoCollectionName)
            ? "user_action_logs"
            : _options.MongoCollectionName.Trim();

        _collection = database.GetCollection<UserActionLogEntry>(collectionName);

        try
        {
            var occurredIndex = new CreateIndexModel<UserActionLogEntry>(
                Builders<UserActionLogEntry>.IndexKeys.Descending(item => item.CompletedAtUtc));
            var actorIndex = new CreateIndexModel<UserActionLogEntry>(
                Builders<UserActionLogEntry>.IndexKeys.Ascending(item => item.ActorUserId));
            var correlationIndex = new CreateIndexModel<UserActionLogEntry>(
                Builders<UserActionLogEntry>.IndexKeys.Ascending(item => item.CorrelationId));
            var traceIndex = new CreateIndexModel<UserActionLogEntry>(
                Builders<UserActionLogEntry>.IndexKeys.Ascending(item => item.TraceId));

            var indexes = new List<CreateIndexModel<UserActionLogEntry>>
            {
                occurredIndex,
                actorIndex,
                correlationIndex,
                traceIndex
            };

            if (_options.RetentionDays > 0)
            {
                var ttlIndex = new CreateIndexModel<UserActionLogEntry>(
                    Builders<UserActionLogEntry>.IndexKeys.Ascending(item => item.CompletedAtUtc),
                    new CreateIndexOptions
                    {
                        Name = "ttl_completed_at",
                        ExpireAfter = TimeSpan.FromDays(_options.RetentionDays)
                    });

                indexes.Add(ttlIndex);
            }

            _collection.Indexes.CreateMany(indexes);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Unable to ensure indexes for user action log collection.");
        }
    }

    public async Task AppendAsync(UserActionLogEntry entry, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled || !_options.PersistToMongo)
        {
            return;
        }

        try
        {
            await _collection.InsertOneAsync(entry, cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Request was aborted by client; skip persistence without noisy warning.
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Failed to persist user action log. CorrelationId: {CorrelationId}, Path: {Path}",
                entry.CorrelationId,
                entry.Path);
        }
    }

    public async Task<UserActionLogSearchResult> SearchAsync(UserActionLogQuery query, CancellationToken cancellationToken = default)
    {
        var filterBuilder = Builders<UserActionLogEntry>.Filter;
        var filters = new List<FilterDefinition<UserActionLogEntry>>();

        if (!string.IsNullOrWhiteSpace(query.ActorUserId))
        {
            filters.Add(filterBuilder.Eq(item => item.ActorUserId, query.ActorUserId.Trim()));
        }

        if (!string.IsNullOrWhiteSpace(query.CorrelationId))
        {
            filters.Add(filterBuilder.Eq(item => item.CorrelationId, query.CorrelationId.Trim()));
        }

        if (!string.IsNullOrWhiteSpace(query.TraceId))
        {
            filters.Add(filterBuilder.Eq(item => item.TraceId, query.TraceId.Trim()));
        }

        if (!string.IsNullOrWhiteSpace(query.Method))
        {
            filters.Add(filterBuilder.Eq(item => item.Method, query.Method.Trim().ToUpperInvariant()));
        }

        if (!string.IsNullOrWhiteSpace(query.PathContains))
        {
            filters.Add(filterBuilder.Regex(item => item.Path, new MongoDB.Bson.BsonRegularExpression(query.PathContains.Trim(), "i")));
        }

        if (query.MinStatusCode.HasValue)
        {
            filters.Add(filterBuilder.Gte(item => item.StatusCode, query.MinStatusCode.Value));
        }

        if (query.MaxStatusCode.HasValue)
        {
            filters.Add(filterBuilder.Lte(item => item.StatusCode, query.MaxStatusCode.Value));
        }

        if (query.FromUtc.HasValue)
        {
            filters.Add(filterBuilder.Gte(item => item.CompletedAtUtc, query.FromUtc.Value));
        }

        if (query.ToUtc.HasValue)
        {
            filters.Add(filterBuilder.Lte(item => item.CompletedAtUtc, query.ToUtc.Value));
        }

        var finalFilter = filters.Count == 0
            ? filterBuilder.Empty
            : filterBuilder.And(filters);

        var skip = (query.Page - 1) * query.PageSize;
        if (skip < 0)
        {
            skip = 0;
        }

        var total = await _collection.CountDocumentsAsync(finalFilter, cancellationToken: cancellationToken);

        var items = await _collection
            .Find(finalFilter)
            .SortByDescending(item => item.CompletedAtUtc)
            .Skip(skip)
            .Limit(query.PageSize)
            .ToListAsync(cancellationToken);

        return new UserActionLogSearchResult(items, total, query.Page, query.PageSize);
    }
}
