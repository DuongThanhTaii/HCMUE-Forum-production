namespace UniHub.Infrastructure.Caching;

/// <summary>
/// Constants for cache keys used throughout the application.
/// </summary>
public static class CacheKeys
{
    /// <summary>
    /// Cache key prefix for user data.
    /// </summary>
    public const string UserPrefix = "user:";

    /// <summary>
    /// Cache key prefix for post data.
    /// </summary>
    public const string PostPrefix = "post:";

    /// <summary>
    /// Cache key prefix for forum data.
    /// </summary>
    public const string ForumPrefix = "forum:";

    /// <summary>
    /// Cache key prefix for notification data.
    /// </summary>
    public const string NotificationPrefix = "notification:";

    /// <summary>
    /// Cache key prefix for course data.
    /// </summary>
    public const string CoursePrefix = "course:";

    /// <summary>
    /// Cache key prefix for chat data.
    /// </summary>
    public const string ChatPrefix = "chat:";

    /// <summary>
    /// Gets a cache key for a specific user by ID.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>The cache key.</returns>
    public static string User(Guid userId) => $"{UserPrefix}{userId}";

    /// <summary>
    /// Gets a cache key for a specific post by ID.
    /// </summary>
    /// <param name="postId">The post ID.</param>
    /// <returns>The cache key.</returns>
    public static string Post(Guid postId) => $"{PostPrefix}{postId}";

    /// <summary>
    /// Gets a cache key for a specific forum by ID.
    /// </summary>
    /// <param name="forumId">The forum ID.</param>
    /// <returns>The cache key.</returns>
    public static string Forum(Guid forumId) => $"{ForumPrefix}{forumId}";

    /// <summary>
    /// Gets a cache key for user notifications.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>The cache key.</returns>
    public static string UserNotifications(Guid userId) => $"{NotificationPrefix}{userId}";

    /// <summary>
    /// Gets a cache key for a specific course by ID.
    /// </summary>
    /// <param name="courseId">The course ID.</param>
    /// <returns>The cache key.</returns>
    public static string Course(Guid courseId) => $"{CoursePrefix}{courseId}";

    /// <summary>
    /// Gets a cache key for user chat conversations.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>The cache key.</returns>
    public static string UserChats(Guid userId) => $"{ChatPrefix}{userId}";

    /// <summary>
    /// Gets a pattern for removing all cache entries with a specific prefix.
    /// </summary>
    /// <param name="prefix">The prefix.</param>
    /// <returns>The pattern.</returns>
    public static string Pattern(string prefix) => $"{prefix}*";
}
