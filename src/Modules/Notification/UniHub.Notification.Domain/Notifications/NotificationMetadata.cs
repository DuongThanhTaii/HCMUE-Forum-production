using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Notification.Domain.Notifications;

/// <summary>
/// Value object representing metadata for a notification.
/// Stores key-value pairs for dynamic data like {UserName}, {JobTitle}, etc.
/// </summary>
public sealed class NotificationMetadata : ValueObject
{
    private readonly Dictionary<string, string> _data;

    /// <summary>Metadata key-value pairs.</summary>
    public IReadOnlyDictionary<string, string> Data => _data;

    public const int MaxKeys = 50;
    public const int MaxKeyLength = 100;
    public const int MaxValueLength = 1000;

    /// <summary>Private constructor for EF Core.</summary>
    private NotificationMetadata()
    {
        _data = new Dictionary<string, string>();
    }

    private NotificationMetadata(Dictionary<string, string> data)
    {
        _data = data;
    }

    /// <summary>
    /// Creates a new NotificationMetadata value object.
    /// </summary>
    public static Result<NotificationMetadata> Create(Dictionary<string, string>? data = null)
    {
        data ??= new Dictionary<string, string>();

        if (data.Count > MaxKeys)
            return Result.Failure<NotificationMetadata>(
                new Error("NotificationMetadata.TooManyKeys",
                    $"Metadata cannot exceed {MaxKeys} key-value pairs."));

        foreach (var kvp in data)
        {
            if (string.IsNullOrWhiteSpace(kvp.Key))
                return Result.Failure<NotificationMetadata>(
                    new Error("NotificationMetadata.EmptyKey", "Metadata key cannot be empty."));

            if (kvp.Key.Length > MaxKeyLength)
                return Result.Failure<NotificationMetadata>(
                    new Error("NotificationMetadata.KeyTooLong",
                        $"Metadata key cannot exceed {MaxKeyLength} characters."));

            if (kvp.Value?.Length > MaxValueLength)
                return Result.Failure<NotificationMetadata>(
                    new Error("NotificationMetadata.ValueTooLong",
                        $"Metadata value cannot exceed {MaxValueLength} characters."));
        }

        // Create a defensive copy
        var dataCopy = new Dictionary<string, string>(data, StringComparer.OrdinalIgnoreCase);

        return Result.Success(new NotificationMetadata(dataCopy));
    }

    /// <summary>
    /// Creates an empty metadata object.
    /// </summary>
    public static NotificationMetadata Empty() => new(new Dictionary<string, string>());

    /// <summary>
    /// Gets a metadata value by key (case-insensitive).
    /// </summary>
    public string? GetValue(string key)
    {
        return _data.TryGetValue(key, out var value) ? value : null;
    }

    /// <summary>
    /// Checks if a key exists (case-insensitive).
    /// </summary>
    public bool ContainsKey(string key) => _data.ContainsKey(key);

    public override string ToString()
        => _data.Count > 0 ? $"{_data.Count} metadata items" : "No metadata";

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        foreach (var kvp in _data.OrderBy(x => x.Key))
        {
            yield return kvp.Key;
            yield return kvp.Value;
        }
    }
}
