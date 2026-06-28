using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using UniHub.SharedKernel.Domain;
// Identity
using UniHub.Identity.Domain.Users;
using UniHub.Chat.Domain.Safety;
using UniHub.Identity.Domain.Roles;
using UniHub.Identity.Domain.Permissions;
using UniHub.Identity.Domain.Tokens;
// Forum
using UniHub.Forum.Domain.Posts;
using UniHub.Forum.Domain.Comments;
using UniHub.Forum.Domain.Categories;
using UniHub.Forum.Domain.Tags;
using UniHub.Forum.Domain.Reports;
using UniHub.Forum.Domain.Bookmarks;
using UniHub.Forum.Domain.ThreadChannels;
// Learning
using UniHub.Learning.Domain.Courses;
using UniHub.Learning.Domain.Documents;
using UniHub.Learning.Domain.Faculties;
// Chat
using UniHub.Chat.Domain.Conversations;
using UniHub.Chat.Domain.Messages;
using UniHub.Chat.Domain.Channels;
// Career
using UniHub.Career.Domain.Companies;
using UniHub.Career.Domain.JobPostings;
using UniHub.Career.Domain.Applications;
using UniHub.Career.Domain.Recruiters;
// Notification
using UniHub.Notification.Domain.Notifications;
using UniHub.Notification.Domain.NotificationPreferences;
using UniHub.Notification.Domain.NotificationTemplates;

namespace UniHub.Infrastructure.Persistence;

/// <summary>
/// Main database context for the application using PostgreSQL.
/// </summary>
public class ApplicationDbContext : DbContext
{
    private readonly bool _useInMemoryNotificationMetadataFix;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class.
    /// </summary>
    /// <param name="options">The options to configure the context.</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
        // Never touch Database.* inside OnModelCreating — it can re-enter initialization.
        // Detect InMemory provider via options extensions (tests use UseInMemoryDatabase).
        _useInMemoryNotificationMetadataFix = options.Extensions.Any(e =>
            e.GetType().FullName?.Contains("InMemory", StringComparison.Ordinal) == true);
    }

    #region Identity Module DbSets
    public DbSet<User> Users => Set<User>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<UserBlock> UserBlocks => Set<UserBlock>();
    #endregion

    #region Forum Module DbSets
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<PostTag> PostTags => Set<PostTag>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<Bookmark> Bookmarks => Set<Bookmark>();
    public DbSet<ThreadChannel> ThreadChannels => Set<ThreadChannel>();
    #endregion

    #region Learning Module DbSets
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<Faculty> Faculties => Set<Faculty>();
    #endregion

    #region Chat Module DbSets
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Channel> Channels => Set<Channel>();
    public DbSet<ConversationMute> ConversationMutes => Set<ConversationMute>();
    public DbSet<ChatMessageReport> ChatMessageReports => Set<ChatMessageReport>();
    #endregion

    #region Career Module DbSets
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<JobPosting> JobPostings => Set<JobPosting>();
    public DbSet<Application> Applications => Set<Application>();
    public DbSet<Recruiter> Recruiters => Set<Recruiter>();
    #endregion

    #region Notification Module DbSets
    public DbSet<UniHub.Notification.Domain.Notifications.Notification> Notifications => Set<UniHub.Notification.Domain.Notifications.Notification>();
    public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();
    public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();
    #endregion

    /// <summary>
    /// Configures the model for this context.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Unit/integration tests often host only a subset of assemblies. AppDomain scan alone skips
        // UniHub.*.Infrastructure DLLs that were never loaded — Career.Application mapping etc. never applies.
        TryLoadInfrastructureConfigurationAssemblies();

        // Apply entity configurations from all loaded assemblies that contain entity type configurations
        // This will automatically discover and apply configurations from all module Infrastructure assemblies
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.FullName != null &&
                       (a.FullName.Contains("UniHub") && a.FullName.Contains("Infrastructure")))
            .ToList();

        foreach (var assembly in assemblies)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(assembly);
        }

        if (_useInMemoryNotificationMetadataFix)
        {
            // NotificationConfiguration maps Metadata as OwnsOne (_data -> jsonb). InMemory needs a
            // comparable store; Entity.Property(n => n.Metadata) conflicts with that ownership.
            var dictComparer = new ValueComparer<Dictionary<string, string>>(
                (c1, c2) => DictionaryEqualsOrdinalIgnoreCaseKeys(c1, c2),
                c => DictionaryContentHashCode(c),
                c => c == null
                    ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    : new Dictionary<string, string>(c, StringComparer.OrdinalIgnoreCase));

            modelBuilder.Entity<UniHub.Notification.Domain.Notifications.Notification>()
                .OwnsOne(n => n.Metadata, metadata =>
                {
                    metadata.Property<Dictionary<string, string>>("_data")
                        .HasConversion(
                            v => System.Text.Json.JsonSerializer.Serialize(v ?? new Dictionary<string, string>()),
                            v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(
                                      string.IsNullOrEmpty(v) ? "{}" : v)
                                  ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase))
                        .Metadata.SetValueComparer(dictComparer);
                });
        }
    }

    private static bool DictionaryEqualsOrdinalIgnoreCaseKeys(Dictionary<string, string>? a, Dictionary<string, string>? b)
    {
        if (ReferenceEquals(a, b)) return true;
        if (a == null || b == null) return false;
        if (a.Count != b.Count) return false;
        foreach (var kv in a)
        {
            if (!TryGetValueOrdinalIgnoreCase(b, kv.Key, out var bv))
                return false;
            if (!string.Equals(kv.Value, bv, StringComparison.Ordinal))
                return false;
        }
        return true;
    }

    private static bool TryGetValueOrdinalIgnoreCase(
        Dictionary<string, string> dict,
        string key,
        out string value)
    {
        if (dict.TryGetValue(key, out value!))
            return true;
        foreach (var kv in dict)
        {
            if (string.Equals(kv.Key, key, StringComparison.OrdinalIgnoreCase))
            {
                value = kv.Value;
                return true;
            }
        }

        value = null!;
        return false;
    }

    private static int DictionaryContentHashCode(Dictionary<string, string>? d)
    {
        if (d == null || d.Count == 0) return 0;
        var hc = new HashCode();
        foreach (var kv in d.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase))
        {
            hc.Add(StringComparer.OrdinalIgnoreCase.GetHashCode(kv.Key));
            hc.Add(kv.Value?.GetHashCode() ?? 0);
        }
        return hc.ToHashCode();
    }
    /// <summary>
    /// Saves all changes made in this context to the database and dispatches domain events.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The number of state entries written to the database.</returns>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // The interceptors will handle:
        // - Setting audit fields (AuditableEntityInterceptor)
        // - Collecting domain events (DomainEventInterceptor)
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Ensures module Infrastructure assemblies are loaded so <see cref="ModelBuilder.ApplyConfigurationsFromAssembly"/>
    /// picks up Career, Forum, etc. Test hosts must still reference/copy those assemblies into output (project refs).
    /// </summary>
    private static void TryLoadInfrastructureConfigurationAssemblies()
    {
        ReadOnlySpan<string> simpleNames =
        [
            "UniHub.Forum.Infrastructure",
            "UniHub.Identity.Infrastructure",
            "UniHub.Learning.Infrastructure",
            "UniHub.Chat.Infrastructure",
            "UniHub.Career.Infrastructure",
            "UniHub.Notification.Infrastructure",
            "UniHub.AI.Infrastructure",
        ];

        foreach (var name in simpleNames)
        {
            try
            {
                Assembly.Load(new AssemblyName(name));
            }
            catch (FileNotFoundException)
            {
                // Assembly not on probing path (minimal host) — skip.
            }
            catch (BadImageFormatException)
            {
                // Skip invalid load attempts.
            }
        }
    }
}
