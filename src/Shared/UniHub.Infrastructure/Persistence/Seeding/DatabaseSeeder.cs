using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace UniHub.Infrastructure.Persistence.Seeding;

/// <summary>
/// Orchestrates database seeding operations.
/// Applies migrations and seeds initial data.
/// </summary>
public static class DatabaseSeeder
{
    /// <summary>
    /// Applies pending migrations and seeds initial data.
    /// </summary>
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        try
        {
            var highVolumeEnabled = configuration.GetValue<bool>("Seeding:HighVolume:Enabled");
            var skipMigrations = configuration.GetValue<bool>("Seeding:HighVolume:SkipMigrations");

            if (!(highVolumeEnabled && skipMigrations))
            {
                logger.LogInformation("Applying database migrations...");
                await context.Database.MigrateAsync();
                logger.LogInformation("Database migrations applied successfully.");
            }
            else
            {
                logger.LogWarning("Skipping migrations before high-volume seed because Seeding:HighVolume:SkipMigrations=true.");
            }

            // Raw-SQL tracking tables (not EF entities). Must match UserRatingService / UserDownloadService table names.
            // Runs after Migrate when possible, and also when migrations are skipped — idempotent CREATE IF NOT EXISTS.
            await EnsureUserDocumentTrackingTablesAsync(context, logger);
            await EnsureChatSafetyTablesAsync(context, logger);

            if (highVolumeEnabled)
            {
                await HighVolumeDatabaseSeed.SeedAsync(context, configuration, logger);
                logger.LogInformation("High-volume database seeding completed successfully.");
                return;
            }

            // Seed data in order of dependencies
            await IdentitySeed.SeedAsync(context, logger);
            await RolePermissionSeeder.SeedAsync(context, logger); // Phase 2: assign DB permissions to roles
            await ForumSeed.SeedAsync(context, logger);
            await LearningSeed.SeedAsync(context, logger);
            await CareerSeed.SeedAsync(context, logger);
            await LearningBulkSeed.SeedAsync(context, configuration, logger);

            logger.LogInformation("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    /// <summary>
    /// Ensures raw-SQL tracking tables exist (public schema). Not EF-mapped; Learning module services query them by name.
    /// Must match SQL in UserRatingService and UserDownloadService.
    /// </summary>
    private static async Task EnsureUserDocumentTrackingTablesAsync(
        ApplicationDbContext context,
        ILogger logger)
    {
        const string sql = """
            CREATE TABLE IF NOT EXISTS user_document_downloads (
                user_id UUID NOT NULL,
                document_id UUID NOT NULL,
                downloaded_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                CONSTRAINT pk_user_document_downloads PRIMARY KEY (user_id, document_id)
            );
            CREATE INDEX IF NOT EXISTS ix_udd_document ON user_document_downloads (document_id);

            CREATE TABLE IF NOT EXISTS user_document_ratings (
                user_id UUID NOT NULL,
                document_id UUID NOT NULL,
                rating INTEGER NOT NULL CHECK (rating BETWEEN 1 AND 5),
                rated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                CONSTRAINT pk_user_document_ratings PRIMARY KEY (user_id, document_id)
            );
            CREATE INDEX IF NOT EXISTS ix_udr_document ON user_document_ratings (document_id);
            """;

        try
        {
            await context.Database.ExecuteSqlRawAsync(sql);
            logger.LogInformation("Verified user_document_downloads / user_document_ratings tables exist.");
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Could not ensure user document tracking tables; rate/download may fail until DB is updated.");
        }
    }

    /// <summary>
    /// Ensures chat safety tables exist (mute, message reports, user blocks).
    /// Mirrors <c>20260516120000_AddChatSafetyAndUserBlocks</c> and scripts/apply-chat-safety-migration.sql.
    /// </summary>
    private static async Task EnsureChatSafetyTablesAsync(ApplicationDbContext context, ILogger logger)
    {
        const string sql = """
            CREATE TABLE IF NOT EXISTS identity.user_blocks (
                blocker_user_id uuid NOT NULL,
                blocked_user_id uuid NOT NULL,
                created_at timestamp with time zone NOT NULL,
                PRIMARY KEY (blocker_user_id, blocked_user_id)
            );
            CREATE INDEX IF NOT EXISTS ix_user_blocks_blocked_user_id
                ON identity.user_blocks (blocked_user_id);

            CREATE TABLE IF NOT EXISTS chat.conversation_mutes (
                user_id uuid NOT NULL,
                conversation_id uuid NOT NULL,
                is_muted boolean NOT NULL DEFAULT false,
                updated_at timestamp with time zone NOT NULL,
                PRIMARY KEY (user_id, conversation_id)
            );

            CREATE TABLE IF NOT EXISTS chat.message_reports (
                id integer GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                message_id uuid NOT NULL,
                conversation_id uuid NOT NULL,
                reporter_id uuid NOT NULL,
                reason integer NOT NULL,
                description character varying(2000),
                created_at timestamp with time zone NOT NULL
            );
            CREATE UNIQUE INDEX IF NOT EXISTS ix_message_reports_reporter_message
                ON chat.message_reports (reporter_id, message_id);
            CREATE INDEX IF NOT EXISTS ix_message_reports_message_id
                ON chat.message_reports (message_id);
            """;

        try
        {
            await context.Database.ExecuteSqlRawAsync(sql);
            logger.LogInformation("Verified chat safety tables (user_blocks, conversation_mutes, message_reports).");
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Could not ensure chat safety tables; chat mute/block/report APIs may fail until DB is updated.");
        }
    }
}
