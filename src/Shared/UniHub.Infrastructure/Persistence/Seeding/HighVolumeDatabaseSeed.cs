using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace UniHub.Infrastructure.Persistence.Seeding;

internal static class HighVolumeDatabaseSeed
{
    public static async Task SeedAsync(ApplicationDbContext context, IConfiguration configuration, ILogger logger)
    {
        var targetTotalRows = configuration.GetValue<int?>("Seeding:HighVolume:TargetTotalRows") ?? 500_000;
        var forceReseed = configuration.GetValue<bool?>("Seeding:HighVolume:ForceReseed") ?? false;

        var existingUsers = await context.Users.CountAsync();
        if (!forceReseed && existingUsers > 0)
        {
            logger.LogInformation(
                "High-volume seed skipped because data already exists ({Count} users). Set Seeding:HighVolume:ForceReseed=true to rebuild dataset.",
                existingUsers);
            return;
        }

        var plan = SeedVolumePlan.Create(targetTotalRows);
        var batchSize = configuration.GetValue<int?>("Seeding:HighVolume:BatchSize") ?? 5_000;
        if (batchSize < 500)
        {
            batchSize = 500;
        }

        logger.LogInformation(
            "Starting high-volume seed. Target={TargetRows}, Planned={PlannedRows}, BatchSize={BatchSize}",
            targetTotalRows,
            plan.TotalRows,
            batchSize);

        context.Database.SetCommandTimeout(TimeSpan.FromMinutes(30));

        await ExecutePhaseAsync("TRUNCATE", logger, () => ExecuteNonQueryAsync(context, BuildTruncateSql()));
        await ExecutePhaseAsync("SETUP_UUID_FN", logger, () => ExecuteNonQueryAsync(context, CreateUuidFunctionSql));

        await ExecutePhaseAsync("IDENTITY_BASE", logger, async () =>
        {
            await ExecuteNonQueryAsync(context, BuildIdentityRolesSql());
            await ExecuteNonQueryAsync(context, BuildIdentityPermissionsSql(plan));
            await SeedInBatchesAsync(context, logger, "identity.users", 1, plan.Users, batchSize, (start, end) => BuildIdentityUsersSql(start, end));
            await SeedInBatchesAsync(context, logger, "identity.user_roles", 1, plan.UserRoles, batchSize, (start, end) => BuildIdentityUserRolesSql(plan, start, end));
            await SeedInBatchesAsync(context, logger, "identity.role_permissions", 1, plan.Permissions, batchSize, (start, end) => BuildIdentityRolePermissionsSql(start, end));
            await SeedInBatchesAsync(context, logger, "identity.refresh_tokens", 1, plan.RefreshTokens, batchSize, (start, end) => BuildIdentityRefreshTokensSql(plan, start, end));
            await SeedInBatchesAsync(context, logger, "identity.password_reset_tokens", 1, plan.PasswordResetTokens, batchSize, (start, end) => BuildIdentityPasswordResetTokensSql(plan, start, end));
        });

        await ExecutePhaseAsync("FORUM_BASE", logger, async () =>
        {
            await ExecuteNonQueryAsync(context, BuildForumCategoriesSql(plan));
            await ExecuteNonQueryAsync(context, BuildForumTagsSql(plan));
            await SeedInBatchesAsync(context, logger, "forum.posts", 1, plan.Posts, batchSize, (start, end) => BuildForumPostsSql(plan, start, end));
            await SeedInBatchesAsync(context, logger, "forum.post_tags", 1, plan.PostTags, batchSize, (start, end) => BuildForumPostTagsSql(plan, start, end));
            await SeedInBatchesAsync(context, logger, "forum.comments", 1, plan.Comments, batchSize, (start, end) => BuildForumCommentsSql(plan, start, end));
            await SeedInBatchesAsync(context, logger, "forum.post_votes", 1, plan.PostVotes, batchSize, (start, end) => BuildForumPostVotesSql(plan, start, end));
            await SeedInBatchesAsync(context, logger, "forum.bookmarks", 1, plan.Bookmarks, batchSize, (start, end) => BuildForumBookmarksSql(plan, start, end));
            await SeedInBatchesAsync(context, logger, "forum.comment_votes", 1, plan.CommentVotes, batchSize, (start, end) => BuildForumCommentVotesSql(plan, start, end));
            await SeedInBatchesAsync(context, logger, "forum.reports", 1, plan.Reports, batchSize, (start, end) => BuildForumReportsSql(plan, start, end));
        });

        await ExecutePhaseAsync("LEARNING_BASE", logger, async () =>
        {
            await ExecuteNonQueryAsync(context, BuildLearningFacultiesSql(plan));
            await ExecuteNonQueryAsync(context, BuildLearningCoursesSql(plan));
            await SeedInBatchesAsync(context, logger, "learning.documents", 1, plan.Documents, batchSize, (start, end) => BuildLearningDocumentsSql(plan, start, end));
        });

        await ExecutePhaseAsync("CAREER_BASE", logger, async () =>
        {
            await ExecuteNonQueryAsync(context, BuildCareerCompaniesSql(plan));
            await ExecuteNonQueryAsync(context, BuildCareerJobPostingsSql(plan));
            await SeedInBatchesAsync(context, logger, "career.job_posting_requirements", 1, plan.JobPostingRequirements, batchSize, (start, end) => BuildCareerJobPostingRequirementsSql(plan, start, end));
            await SeedInBatchesAsync(context, logger, "career.applications", 1, plan.Applications, batchSize, (start, end) => BuildCareerApplicationsSql(plan, start, end));
            await SeedInBatchesAsync(context, logger, "career.recruiters", 1, plan.Recruiters, batchSize, (start, end) => BuildCareerRecruitersSql(plan, start, end));
        });

        await ExecutePhaseAsync("CHAT_BASE", logger, async () =>
        {
            await ExecuteNonQueryAsync(context, BuildChatConversationsSql(plan));
            await ExecuteNonQueryAsync(context, BuildChatChannelsSql(plan));
            await SeedInBatchesAsync(context, logger, "chat.messages", 1, plan.Messages, batchSize, (start, end) => BuildChatMessagesSql(plan, start, end));
            await SeedInBatchesAsync(context, logger, "chat.message_attachments", 1, plan.MessageAttachments, batchSize, (start, end) => BuildChatMessageAttachmentsSql(plan, start, end));
            await SeedInBatchesAsync(context, logger, "chat.message_reactions", 1, plan.MessageReactions, batchSize, (start, end) => BuildChatMessageReactionsSql(plan, start, end));
            await SeedInBatchesAsync(context, logger, "chat.message_read_receipts", 1, plan.MessageReadReceipts, batchSize, (start, end) => BuildChatMessageReadReceiptsSql(plan, start, end));
        });

        await ExecutePhaseAsync("NOTIFICATION_BASE", logger, async () =>
        {
            await ExecuteNonQueryAsync(context, BuildNotificationTemplatesSql(plan));
            await SeedInBatchesAsync(context, logger, "notification.notification_template_variables", 1, plan.NotificationTemplateVariables, batchSize, (start, end) => BuildNotificationTemplateVariablesSql(plan, start, end));
            await SeedInBatchesAsync(context, logger, "notification.notification_preferences", 1, plan.NotificationPreferences, batchSize, (start, end) => BuildNotificationPreferencesSql(start, end));
            await SeedInBatchesAsync(context, logger, "notification.notifications", 1, plan.Notifications, batchSize, (start, end) => BuildNotificationsSql(plan, start, end));
        });

        await ExecutePhaseAsync("CLEANUP_UUID_FN", logger, () => ExecuteNonQueryAsync(context, DropUuidFunctionSql));

        logger.LogInformation(
            "High-volume seed completed. users={Users}, posts={Posts}, comments={Comments}, messages={Messages}, notifications={Notifications}",
            plan.Users,
            plan.Posts,
            plan.Comments,
            plan.Messages,
            plan.Notifications);
    }

    private static async Task ExecutePhaseAsync(string phaseName, ILogger logger, Func<Task> action)
    {
        var sw = Stopwatch.StartNew();
        logger.LogInformation("[SeedPhase:{Phase}] START", phaseName);
        try
        {
            await action();
            sw.Stop();
            logger.LogInformation("[SeedPhase:{Phase}] DONE in {ElapsedMs} ms", phaseName, sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogError(ex, "[SeedPhase:{Phase}] FAILED after {ElapsedMs} ms", phaseName, sw.ElapsedMilliseconds);
            throw;
        }
    }

    private static async Task SeedInBatchesAsync(
        ApplicationDbContext context,
        ILogger logger,
        string stepName,
        int from,
        int to,
        int batchSize,
        Func<int, int, string> sqlFactory)
    {
        if (to < from)
        {
            return;
        }

        var total = to - from + 1;
        var batchCount = (int)Math.Ceiling(total / (double)batchSize);

        for (var batchIndex = 0; batchIndex < batchCount; batchIndex++)
        {
            var start = from + (batchIndex * batchSize);
            var end = Math.Min(to, start + batchSize - 1);
            var sw = Stopwatch.StartNew();

            logger.LogInformation("[SeedBatch:{Step}] {Batch}/{TotalBatches} range={Start}-{End}", stepName, batchIndex + 1, batchCount, start, end);
            await ExecuteNonQueryAsync(context, sqlFactory(start, end));

            sw.Stop();
            logger.LogInformation("[SeedBatch:{Step}] done in {ElapsedMs} ms", stepName, sw.ElapsedMilliseconds);
        }
    }

    private static Task<int> ExecuteNonQueryAsync(ApplicationDbContext context, string sql)
    {
        return context.Database.ExecuteSqlRawAsync(sql);
    }

    private const string CreateUuidFunctionSql = """
CREATE OR REPLACE FUNCTION public.seed_uuid(seed text, idx bigint)
RETURNS uuid
LANGUAGE SQL
IMMUTABLE
AS $$
SELECT (
    substr(md5(seed || ':' || idx::text), 1, 8) || '-' ||
    substr(md5(seed || ':' || idx::text), 9, 4) || '-' ||
    substr(md5(seed || ':' || idx::text), 13, 4) || '-' ||
    substr(md5(seed || ':' || idx::text), 17, 4) || '-' ||
    substr(md5(seed || ':' || idx::text), 21, 12)
)::uuid;
$$;
""";

    private const string DropUuidFunctionSql = "DROP FUNCTION IF EXISTS public.seed_uuid(text, bigint);";

    private static string BuildTruncateSql() => """
TRUNCATE TABLE
forum.comment_votes,
forum.post_votes,
forum.post_tags,
forum.bookmarks,
forum.comments,
forum.reports,
forum.posts,
forum.tags,
forum.categories,
chat.message_attachments,
chat.message_reactions,
chat.message_read_receipts,
chat.messages,
chat.channels,
chat.conversations,
career.job_posting_requirements,
career.applications,
career.recruiters,
career.job_postings,
career.companies,
learning.documents,
learning.courses,
learning.faculties,
notification.notification_template_variables,
notification.notifications,
notification.notification_preferences,
notification.notification_templates,
identity.refresh_tokens,
identity.password_reset_tokens,
identity.user_roles,
identity.role_permissions,
identity.permissions,
identity.roles,
identity.users
RESTART IDENTITY CASCADE;
""";

    private static string BuildIdentityRolesSql() => """
INSERT INTO identity.roles (id, name, description, is_default, is_system_role, created_at)
VALUES
    (public.seed_uuid('role', 1), 'Admin', 'System administrator role', false, true, now()),
    (public.seed_uuid('role', 2), 'Moderator', 'Forum moderator role', false, true, now()),
    (public.seed_uuid('role', 3), 'Lecturer', 'Lecturer role', false, false, now()),
    (public.seed_uuid('role', 4), 'Student', 'Student role', true, false, now()),
    (public.seed_uuid('role', 5), 'Recruiter', 'Recruiter role', false, false, now()),
    (public.seed_uuid('role', 6), 'Support', 'Support role', false, true, now());
""";

    private static string BuildIdentityPermissionsSql(SeedVolumePlan p) => $"""
INSERT INTO identity.permissions (id, code, name, description, module, resource, action, created_at)
SELECT
    public.seed_uuid('permission', i),
    'seed.module' || ((i - 1) % 12 + 1)::text || '.resource' || ((i - 1) % 18 + 1)::text || '.action' || i::text,
    'Seed Permission ' || i::text,
    'Synthetic permission for load testing',
    'seed_module_' || ((i - 1) % 12 + 1)::text,
    'resource_' || ((i - 1) % 18 + 1)::text,
    'action_' || i::text,
    now()
FROM generate_series(1, {p.Permissions}) AS g(i);
""";

    private static string BuildIdentityUsersSql(int start, int end) => $"""
INSERT INTO identity.users (id, email, password_hash, first_name, last_name, status, created_at)
SELECT
    public.seed_uuid('user', i),
    'user' || lpad(i::text, 6, '0') || '@seed.unihub.local',
    '$2a$12$k312te0PvwsBFoDQ0i9y2ufy5.gzcWlsZDVh5JqVzyrHPgH5bNGbK',
    'User',
    lpad(i::text, 6, '0'),
    1,
    now() - make_interval(days => (i % 365))
FROM generate_series({start}, {end}) AS g(i);
""";

    private static string BuildIdentityUserRolesSql(SeedVolumePlan p, int start, int end) => $"""
INSERT INTO identity.user_roles (id, user_id, role_id, assigned_at)
SELECT
    public.seed_uuid('user_role', i),
    public.seed_uuid('user', i),
    public.seed_uuid('role', ((i - 1) % 4) + 1),
    now() - make_interval(days => (i % 120))
FROM generate_series({start}, {end}) AS g(i);
""";

    private static string BuildIdentityRolePermissionsSql(int start, int end) => $"""
INSERT INTO identity.role_permissions (id, role_id, permission_id, scope_type, scope_value, assigned_at)
SELECT
    public.seed_uuid('role_permission', i),
    public.seed_uuid('role', ((i - 1) % 6) + 1),
    public.seed_uuid('permission', i),
    0,
    NULL,
    now() - make_interval(days => (i % 60))
FROM generate_series({start}, {end}) AS g(i);
""";

    private static string BuildIdentityRefreshTokensSql(SeedVolumePlan p, int start, int end) => $"""
INSERT INTO identity.refresh_tokens (id, token, expires_at, created_at, created_by_ip, user_id)
SELECT
    public.seed_uuid('refresh_token', i),
    'refresh-token-' || i::text,
    now() + make_interval(days => 14),
    now() - make_interval(hours => (i % 240)),
    '127.0.0.' || ((i - 1) % 200 + 1)::text,
    public.seed_uuid('user', ((i - 1) % {p.Users}) + 1)
FROM generate_series({start}, {end}) AS g(i);
""";

    private static string BuildIdentityPasswordResetTokensSql(SeedVolumePlan p, int start, int end) => $"""
INSERT INTO identity.password_reset_tokens (id, user_id, token, expires_at, created_at, is_used, used_at)
SELECT
    public.seed_uuid('password_reset', i),
    public.seed_uuid('user', ((i - 1) % {p.Users}) + 1),
    'password-reset-token-' || i::text,
    now() + make_interval(hours => 3),
    now() - make_interval(hours => (i % 48)),
    false,
    NULL
FROM generate_series({start}, {end}) AS g(i);
""";

    private static string BuildForumCategoriesSql(SeedVolumePlan p) => $"""
INSERT INTO forum.categories (id, name, description, slug, parent_category_id, post_count, display_order, is_active, created_at, moderator_ids)
SELECT
    public.seed_uuid('category', i),
    'Category ' || lpad(i::text, 3, '0'),
    'Category seeded for load testing #' || i::text,
    'category-' || lpad(i::text, 3, '0'),
    CASE WHEN i > 5 AND i % 5 = 0 THEN public.seed_uuid('category', i - 1) ELSE NULL END,
    0,
    i,
    true,
    now() - make_interval(days => (i % 90)),
    '[]'::jsonb
FROM generate_series(1, {p.Categories}) AS g(i);
""";

    private static string BuildForumTagsSql(SeedVolumePlan p) => $"""
INSERT INTO forum.tags (id, name, description, slug, usage_count, created_at)
SELECT
    i,
    'tag_' || lpad(i::text, 4, '0'),
    'Seed tag ' || i::text,
    'tag-' || lpad(i::text, 4, '0'),
    0,
    now() - make_interval(days => (i % 120))
FROM generate_series(1, {p.Tags}) AS g(i);

SELECT setval(pg_get_serial_sequence('forum.tags', 'id'), {p.Tags}, true);
""";

    private static string BuildForumPostsSql(SeedVolumePlan p, int start, int end) => $"""
INSERT INTO forum.posts (id, title, content, slug, type, status, author_id, category_id, view_count, vote_score, comment_count, is_pinned, is_locked, created_at, published_at, tags)
SELECT
    public.seed_uuid('post', i),
    'Seed Post #' || i::text,
    repeat('Seed content for post #' || i::text || ' ', 10),
    'seed-post-' || i::text,
    0,
    1,
    public.seed_uuid('user', ((i - 1) % {p.Users}) + 1),
    public.seed_uuid('category', ((i - 1) % {p.Categories}) + 1),
    (i % 2000)::int,
    ((i % 200) - 100)::int,
    0,
    (i % 250 = 0),
    false,
    now() - make_interval(days => (i % 365)),
    now() - make_interval(days => (i % 364)),
    '[]'::jsonb
FROM generate_series({start}, {end}) AS g(i);
""";

    private static string BuildForumPostTagsSql(SeedVolumePlan p, int start, int end) => $"""
INSERT INTO forum.post_tags (post_id, tag_id, added_at)
SELECT
    public.seed_uuid('post', ((i - 1) % {p.Posts}) + 1),
    ((((i - 1) / {p.Posts}) % {p.Tags}) + 1)::int,
    now() - make_interval(days => (i % 180))
FROM generate_series({start}, {end}) AS g(i);
""";

    private static string BuildForumCommentsSql(SeedVolumePlan p, int start, int end) => $"""
INSERT INTO forum.comments (id, post_id, author_id, content, parent_comment_id, is_accepted_answer, vote_score, is_deleted, created_at)
SELECT
    public.seed_uuid('comment', i),
    public.seed_uuid('post', ((i - 1) % {p.Posts}) + 1),
    public.seed_uuid('user', ((i - 1) % {p.Users}) + 1),
    'Seed comment #' || i::text,
    CASE WHEN i > 10 AND i % 10 = 0 THEN public.seed_uuid('comment', i - 1) ELSE NULL END,
    (i % 200 = 0),
    ((i % 40) - 20)::int,
    false,
    now() - make_interval(days => (i % 240))
FROM generate_series({start}, {end}) AS g(i);
""";

    private static string BuildForumPostVotesSql(SeedVolumePlan p, int start, int end) => $"""
INSERT INTO forum.post_votes (user_id, post_id, vote_type, created_at)
SELECT
    public.seed_uuid('user', (((i - 1) / {p.Posts}) % {p.Users}) + 1),
    public.seed_uuid('post', ((i - 1) % {p.Posts}) + 1),
    CASE WHEN i % 4 = 0 THEN 2 ELSE 1 END,
    now() - make_interval(days => (i % 120))
FROM generate_series({start}, {end}) AS g(i);
""";

    private static string BuildForumBookmarksSql(SeedVolumePlan p, int start, int end) => $"""
INSERT INTO forum.bookmarks (post_id, user_id, created_at)
SELECT
    public.seed_uuid('post', ((i - 1) % {p.Posts}) + 1),
    public.seed_uuid('user', (((i - 1) / {p.Posts}) % {p.Users}) + 1),
    now() - make_interval(days => (i % 180))
FROM generate_series({start}, {end}) AS g(i);
""";

    private static string BuildForumCommentVotesSql(SeedVolumePlan p, int start, int end) => $"""
INSERT INTO forum.comment_votes (user_id, comment_id, vote_type, created_at)
SELECT
    public.seed_uuid('user', (((i - 1) / {p.Comments}) % {p.Users}) + 1),
    public.seed_uuid('comment', ((i - 1) % {p.Comments}) + 1),
    CASE WHEN i % 5 = 0 THEN 2 ELSE 1 END,
    now() - make_interval(days => (i % 120))
FROM generate_series({start}, {end}) AS g(i);
""";

    private static string BuildForumReportsSql(SeedVolumePlan p, int start, int end) => $"""
INSERT INTO forum.reports (reported_item_id, reported_item_type, reporter_id, reason, description, status, created_at)
SELECT
    CASE WHEN i % 2 = 0 THEN public.seed_uuid('post', ((i - 1) % {p.Posts}) + 1) ELSE public.seed_uuid('comment', ((i - 1) % {p.Comments}) + 1) END,
    CASE WHEN i % 2 = 0 THEN 0 ELSE 1 END,
    public.seed_uuid('user', ((i - 1) % {p.Users}) + 1),
    (i % 6)::int,
    'Auto-generated moderation report #' || i::text,
    CASE WHEN i % 8 = 0 THEN 1 ELSE 0 END,
    now() - make_interval(days => (i % 180))
FROM generate_series({start}, {end}) AS g(i);
""";

    private static string BuildLearningFacultiesSql(SeedVolumePlan p) => $"""
INSERT INTO learning.faculties (id, code, name, description, status, manager_id, course_count, created_at, created_by)
SELECT
    public.seed_uuid('faculty', i),
    'FAC' || lpad(i::text, 3, '0'),
    'Faculty ' || i::text,
    'Seed faculty #' || i::text,
    1,
    public.seed_uuid('user', ((i - 1) % {p.Users}) + 1),
    0,
    now() - make_interval(days => (i % 365)),
    public.seed_uuid('user', ((i - 1) % {p.Users}) + 1)
FROM generate_series(1, {p.Faculties}) AS g(i);
""";

    private static string BuildLearningCoursesSql(SeedVolumePlan p) => $"""
INSERT INTO learning.courses (id, code, name, description, semester, status, faculty_id, credits, document_count, enrollment_count, created_at, created_by, moderator_ids)
SELECT
    public.seed_uuid('course', i),
    'COURSE' || lpad(i::text, 5, '0'),
    'Course ' || i::text,
    'Seed course #' || i::text,
    '2026A',
    1,
    public.seed_uuid('faculty', ((i - 1) % {p.Faculties}) + 1),
    ((i - 1) % 5) + 2,
    0,
    ((i - 1) % 500) + 10,
    now() - make_interval(days => (i % 365)),
    public.seed_uuid('user', ((i - 1) % {p.Users}) + 1),
    '[]'::jsonb
FROM generate_series(1, {p.Courses}) AS g(i);
""";

    private static string BuildLearningDocumentsSql(SeedVolumePlan p, int start, int end) => $"""
INSERT INTO learning.documents (id, title, description, file_name, file_path, file_size, content_type, file_extension, type, status, uploader_id, course_id, reviewer_id, review_comment, rejection_reason, download_count, view_count, average_rating, rating_count, created_at, submitted_at, reviewed_at)
SELECT
    public.seed_uuid('document', i),
    'Document ' || i::text,
    'Seed learning document #' || i::text,
    'doc-' || i::text || '.pdf',
    '/seed/docs/doc-' || i::text || '.pdf',
    1024 + (i % 2048),
    'application/pdf',
    '.pdf',
    0,
    1,
    public.seed_uuid('user', ((i - 1) % {p.Users}) + 1),
    public.seed_uuid('course', ((i - 1) % {p.Courses}) + 1),
    public.seed_uuid('user', ((i + 13 - 1) % {p.Users}) + 1),
    NULL,
    NULL,
    (i % 500)::int,
    (i % 2000)::int,
    ((i % 50)::numeric / 10.0),
    (i % 300)::int,
    now() - make_interval(days => (i % 365)),
    now() - make_interval(days => (i % 364)),
    now() - make_interval(days => (i % 363))
FROM generate_series({start}, {end}) AS g(i);
""";

    private static string BuildCareerCompaniesSql(SeedVolumePlan p) => $"""
INSERT INTO career.companies (id, name, description, industry, size, website, logo_url, founded_year, status, contact_email, registered_by, registered_at, total_job_postings, benefits)
SELECT
    public.seed_uuid('company', i),
    'Company ' || (ARRAY['Tech', 'Solutions', 'Systems', 'Global', 'Innovations', 'Group', 'Networks', 'Labs'])[i % 8 + 1] || ' ' || i::text,
    'Leading provider of ' || (ARRAY['software', 'hardware', 'cloud', 'AI', 'data', 'security', 'fintech', 'edtech'])[i % 8 + 1] || ' solutions.',
    (i % 8)::int,
    (i % 5)::int,
    'https://company' || i::text || '.example.com',
    NULL,
    2000 + (i % 25),
    1,
    'hr+' || i::text || '@company-seed.local',
    public.seed_uuid('user', ((i - 1) % {p.Users}) + 1),
    now() - make_interval(days => (i % 700)),
    0,
    '[]'::jsonb
FROM generate_series(1, {p.Companies}) AS g(i);
""";

    private static string BuildCareerJobPostingsSql(SeedVolumePlan p) => $"""
INSERT INTO career.job_postings (id, title, description, company_id, posted_by, job_type, experience_level, status, salary_min_amount, salary_max_amount, salary_currency, salary_period, location_city, location_district, location_address, location_is_remote, deadline, created_at, published_at, view_count, application_count, tags)
SELECT
    public.seed_uuid('job', i),
    (ARRAY['Software Engineer', 'Data Scientist', 'Product Manager', 'UX Designer', 'DevOps Engineer', 'Frontend Developer', 'Backend Developer', 'Marketing Specialist'])[i % 8 + 1] || ' ' || i::text,
    'We are looking for a skilled ' || (ARRAY['Software Engineer', 'Data Scientist', 'Product Manager', 'UX Designer', 'DevOps Engineer', 'Frontend Developer', 'Backend Developer', 'Marketing Specialist'])[i % 8 + 1] || ' to join our growing team.',
    public.seed_uuid('company', ((i - 1) % {p.Companies}) + 1),
    public.seed_uuid('user', ((i - 1) % {p.Users}) + 1),
    (i % 4)::int,
    (i % 4)::int,
    1,
    500 + (i % 2000),
    1500 + (i % 5000),
    'USD',
    'MONTH',
    'Ho Chi Minh City',
    'District ' || ((i - 1) % 12 + 1)::text,
    'Seed Address ' || i::text,
    (i % 5 = 0),
    now() + make_interval(days => ((i % 60) + 7)),
    now() - make_interval(days => (i % 120)),
    now() - make_interval(days => (i % 119)),
    (i % 2000)::int,
    0,
    CASE (i % 5)
        WHEN 0 THEN '["IT", "Software"]'::jsonb
        WHEN 1 THEN '["Design", "UX"]'::jsonb
        WHEN 2 THEN '["Marketing", "Sales"]'::jsonb
        WHEN 3 THEN '["Data", "AI"]'::jsonb
        ELSE '["Engineering", "DevOps"]'::jsonb
    END
FROM generate_series(1, {p.JobPostings}) AS g(i);
""";

    private static string BuildCareerJobPostingRequirementsSql(SeedVolumePlan p, int start, int end) => $"""
INSERT INTO career.job_posting_requirements (skill, job_posting_id, is_required)
SELECT
    'skill_' || ((((i - 1) / {p.JobPostings}) % 20) + 1)::text,
    public.seed_uuid('job', ((i - 1) % {p.JobPostings}) + 1),
    true
FROM generate_series({start}, {end}) AS g(i);
""";

    private static string BuildCareerApplicationsSql(SeedVolumePlan p, int start, int end) => $"""
INSERT INTO career.applications (id, job_posting_id, applicant_id, status, resume_file_name, resume_file_url, resume_file_size_bytes, resume_content_type, cover_letter_content, submitted_at)
SELECT
    public.seed_uuid('application', i),
    public.seed_uuid('job', ((i - 1) % {p.JobPostings}) + 1),
    public.seed_uuid('user', (((i - 1) / {p.JobPostings}) % {p.Users}) + 1),
    (i % 5)::int,
    'resume-' || i::text || '.pdf',
    'https://cdn.unihub.local/resume-' || i::text || '.pdf',
    1024 + (i % 4096),
    'application/pdf',
    'Cover letter #' || i::text,
    now() - make_interval(days => (i % 180))
FROM generate_series({start}, {end}) AS g(i);
""";

    private static string BuildCareerRecruitersSql(SeedVolumePlan p, int start, int end) => $"""
INSERT INTO career.recruiters (id, user_id, company_id, can_manage_job_postings, can_review_applications, can_update_application_status, can_invite_recruiters, status, added_by, added_at)
SELECT
    public.seed_uuid('recruiter', i),
    public.seed_uuid('user', ((i - 1) % {p.Users}) + 1),
    public.seed_uuid('company', ((i - 1) % {p.Companies}) + 1),
    true,
    true,
    true,
    (i % 3 = 0),
    1,
    public.seed_uuid('user', ((i + 7 - 1) % {p.Users}) + 1),
    now() - make_interval(days => (i % 365))
FROM generate_series({start}, {end}) AS g(i);
""";

    private static string BuildChatConversationsSql(SeedVolumePlan p) => $"""
INSERT INTO chat.conversations (id, type, title, created_by, created_at, last_message_at, is_archived, participants)
SELECT
    public.seed_uuid('conversation', i),
    CASE WHEN i % 10 = 0 THEN 1 ELSE 0 END,
    'Conversation ' || i::text,
    public.seed_uuid('user', ((i - 1) % {p.Users}) + 1),
    now() - make_interval(days => (i % 180)),
    now() - make_interval(hours => (i % 72)),
    false,
    '[]'::jsonb
FROM generate_series(1, {p.Conversations}) AS g(i);
""";

    private static string BuildChatChannelsSql(SeedVolumePlan p) => $"""
INSERT INTO chat.channels (id, name, description, type, owner_id, created_at, is_archived, members, moderators)
SELECT
    public.seed_uuid('channel', i),
    'channel-' || lpad(i::text, 5, '0'),
    'Seed channel #' || i::text,
    0,
    public.seed_uuid('user', ((i - 1) % {p.Users}) + 1),
    now() - make_interval(days => (i % 240)),
    false,
    '[]'::jsonb,
    '[]'::jsonb
FROM generate_series(1, {p.Channels}) AS g(i);
""";

    private static string BuildChatMessagesSql(SeedVolumePlan p, int start, int end) => $"""
INSERT INTO chat.messages (id, conversation_id, sender_id, content, type, sent_at, edited_at, is_deleted, deleted_at, reply_to_message_id)
SELECT
    public.seed_uuid('message', i),
    public.seed_uuid('conversation', ((i - 1) % {p.Conversations}) + 1),
    public.seed_uuid('user', ((i - 1) % {p.Users}) + 1),
    'Seed message #' || i::text,
    0,
    now() - make_interval(hours => (i % 500)),
    NULL,
    false,
    NULL,
    CASE WHEN i > 20 AND i % 20 = 0 THEN public.seed_uuid('message', i - 1) ELSE NULL END
FROM generate_series({start}, {end}) AS g(i);
""";

    private static string BuildChatMessageAttachmentsSql(SeedVolumePlan p, int start, int end) => $"""
INSERT INTO chat.message_attachments (file_name, message_id, file_url, file_size_bytes, mime_type, thumbnail_url)
SELECT
    'attachment-' || i::text || '.png',
    public.seed_uuid('message', ((i - 1) % {p.Messages}) + 1),
    'https://cdn.unihub.local/chat/attachment-' || i::text || '.png',
    2048 + (i % 5000),
    'image/png',
    NULL
FROM generate_series({start}, {end}) AS g(i);
""";

    private static string BuildChatMessageReactionsSql(SeedVolumePlan p, int start, int end) => $"""
INSERT INTO chat.message_reactions (user_id, emoji, message_id, reacted_at)
SELECT
    public.seed_uuid('user', (((i - 1) / {p.Messages}) % {p.Users}) + 1),
    CASE (i % 4) WHEN 0 THEN ':thumbsup:' WHEN 1 THEN ':heart:' WHEN 2 THEN ':fire:' ELSE ':tada:' END,
    public.seed_uuid('message', ((i - 1) % {p.Messages}) + 1),
    now() - make_interval(hours => (i % 240))
FROM generate_series({start}, {end}) AS g(i);
""";

    private static string BuildChatMessageReadReceiptsSql(SeedVolumePlan p, int start, int end) => $"""
INSERT INTO chat.message_read_receipts (user_id, message_id, read_at)
SELECT
    public.seed_uuid('user', (((i - 1) / {p.Messages}) % {p.Users}) + 1),
    public.seed_uuid('message', ((i - 1) % {p.Messages}) + 1),
    now() - make_interval(hours => (i % 180))
FROM generate_series({start}, {end}) AS g(i);
""";

    private static string BuildNotificationTemplatesSql(SeedVolumePlan p) => $"""
INSERT INTO notification.notification_templates (id, name, display_name, description, category, status, email_subject, email_body, inapp_title, inapp_body, created_by, created_at, channels)
SELECT
    public.seed_uuid('notification_template', i),
    'template_' || lpad(i::text, 3, '0'),
    'Template ' || i::text,
    'Seed notification template #' || i::text,
    (i % 4)::int,
    1,
    'Seed subject ' || i::text,
    'Seed email body for template ' || i::text,
    'Seed in-app title ' || i::text,
    'Seed in-app body ' || i::text,
    public.seed_uuid('user', ((i - 1) % {p.Users}) + 1),
    now() - make_interval(days => (i % 120)),
    '[]'::jsonb
FROM generate_series(1, {p.NotificationTemplates}) AS g(i);
""";

    private static string BuildNotificationTemplateVariablesSql(SeedVolumePlan p, int start, int end) => $"""
INSERT INTO notification.notification_template_variables (name, template_id, description, example_value)
SELECT
    'var_' || ((((i - 1) / {p.NotificationTemplates}) % 10) + 1)::text,
    public.seed_uuid('notification_template', ((i - 1) % {p.NotificationTemplates}) + 1),
    'Seed variable #' || i::text,
    'example-' || i::text
FROM generate_series({start}, {end}) AS g(i);
""";

    private static string BuildNotificationPreferencesSql(int start, int end) => $"""
INSERT INTO notification.notification_preferences (id, user_id, email_enabled, push_enabled, in_app_enabled, created_at)
SELECT
    public.seed_uuid('notification_preference', i),
    public.seed_uuid('user', i),
    true,
    true,
    true,
    now() - make_interval(days => (i % 365))
FROM generate_series({start}, {end}) AS g(i);
""";

    private static string BuildNotificationsSql(SeedVolumePlan p, int start, int end) => $"""
INSERT INTO notification.notifications (id, recipient_id, template_id, channel, status, content_subject, content_body, content_action_url, content_icon_url, metadata, created_at, sent_at, read_at, dismissed_at, failure_reason, send_attempts)
SELECT
    public.seed_uuid('notification', i),
    public.seed_uuid('user', ((i - 1) % {p.Users}) + 1),
    public.seed_uuid('notification_template', ((i - 1) % {p.NotificationTemplates}) + 1),
    (i % 3)::int,
    CASE WHEN i % 10 = 0 THEN 3 ELSE 1 END,
    'Notification ' || i::text,
    'Seed notification content #' || i::text,
    CASE WHEN i % 7 = 0 THEN 'https://unihub.local/action/' || i::text ELSE NULL END,
    NULL,
    jsonb_build_object('seed', true, 'index', i),
    now() - make_interval(days => (i % 365)),
    now() - make_interval(days => (i % 364)),
    CASE WHEN i % 3 = 0 THEN now() - make_interval(days => (i % 200)) ELSE NULL END,
    CASE WHEN i % 9 = 0 THEN now() - make_interval(days => (i % 150)) ELSE NULL END,
    CASE WHEN i % 10 = 0 THEN 'Simulated delivery issue' ELSE NULL END,
    CASE WHEN i % 10 = 0 THEN 3 ELSE 1 END
FROM generate_series({start}, {end}) AS g(i);
""";

    private static string BuildSql(SeedVolumePlan p)
    {
        return $"""
TRUNCATE TABLE
forum.comment_votes,
forum.post_votes,
forum.post_tags,
forum.bookmarks,
forum.comments,
forum.reports,
forum.posts,
forum.tags,
forum.categories,
chat.message_attachments,
chat.message_reactions,
chat.message_read_receipts,
chat.messages,
chat.channels,
chat.conversations,
career.job_posting_requirements,
career.applications,
career.recruiters,
career.job_postings,
career.companies,
learning.documents,
learning.courses,
learning.faculties,
notification.notification_template_variables,
notification.notifications,
notification.notification_preferences,
notification.notification_templates,
identity.refresh_tokens,
identity.password_reset_tokens,
identity.user_roles,
identity.role_permissions,
identity.permissions,
identity.roles,
identity.users
RESTART IDENTITY CASCADE;

CREATE OR REPLACE FUNCTION public.seed_uuid(seed text, idx bigint)
RETURNS uuid
LANGUAGE SQL
IMMUTABLE
AS $$
SELECT (
    substr(md5(seed || ':' || idx::text), 1, 8) || '-' ||
    substr(md5(seed || ':' || idx::text), 9, 4) || '-' ||
    substr(md5(seed || ':' || idx::text), 13, 4) || '-' ||
    substr(md5(seed || ':' || idx::text), 17, 4) || '-' ||
    substr(md5(seed || ':' || idx::text), 21, 12)
)::uuid;
$$;

INSERT INTO identity.users (id, email, password_hash, first_name, last_name, status, created_at)
SELECT
    public.seed_uuid('user', i),
    'user' || lpad(i::text, 6, '0') || '@seed.unihub.local',
    '$2a$12$k312te0PvwsBFoDQ0i9y2ufy5.gzcWlsZDVh5JqVzyrHPgH5bNGbK',
    'User',
    lpad(i::text, 6, '0'),
    1,
    now() - make_interval(days => (i % 365))
FROM generate_series(1, {p.Users}) AS g(i);

INSERT INTO identity.roles (id, name, description, is_default, is_system_role, created_at)
VALUES
    (public.seed_uuid('role', 1), 'Admin', 'System administrator role', false, true, now()),
    (public.seed_uuid('role', 2), 'Moderator', 'Forum moderator role', false, true, now()),
    (public.seed_uuid('role', 3), 'Lecturer', 'Lecturer role', false, false, now()),
    (public.seed_uuid('role', 4), 'Student', 'Student role', true, false, now()),
    (public.seed_uuid('role', 5), 'Recruiter', 'Recruiter role', false, false, now()),
    (public.seed_uuid('role', 6), 'Support', 'Support role', false, true, now());

INSERT INTO identity.permissions (id, code, name, description, module, resource, action, created_at)
SELECT
    public.seed_uuid('permission', i),
    'seed.module' || ((i - 1) % 12 + 1)::text || '.resource' || ((i - 1) % 18 + 1)::text || '.action' || i::text,
    'Seed Permission ' || i::text,
    'Synthetic permission for load testing',
    'seed_module_' || ((i - 1) % 12 + 1)::text,
    'resource_' || ((i - 1) % 18 + 1)::text,
    'action_' || i::text,
    now()
FROM generate_series(1, {p.Permissions}) AS g(i);

INSERT INTO identity.user_roles (id, user_id, role_id, assigned_at)
SELECT
    public.seed_uuid('user_role', i),
    public.seed_uuid('user', i),
    public.seed_uuid('role', ((i - 1) % 4) + 1),
    now() - make_interval(days => (i % 120))
FROM generate_series(1, {p.UserRoles}) AS g(i);

INSERT INTO identity.role_permissions (id, role_id, permission_id, scope_type, scope_value, assigned_at)
SELECT
    public.seed_uuid('role_permission', i),
    public.seed_uuid('role', ((i - 1) % 6) + 1),
    public.seed_uuid('permission', i),
    0,
    NULL,
    now() - make_interval(days => (i % 60))
FROM generate_series(1, {p.Permissions}) AS g(i);

INSERT INTO identity.refresh_tokens (id, token, expires_at, created_at, created_by_ip, user_id)
SELECT
    public.seed_uuid('refresh_token', i),
    'refresh-token-' || i::text,
    now() + make_interval(days => 14),
    now() - make_interval(hours => (i % 240)),
    '127.0.0.' || ((i - 1) % 200 + 1)::text,
    public.seed_uuid('user', ((i - 1) % {p.Users}) + 1)
FROM generate_series(1, {p.RefreshTokens}) AS g(i);

INSERT INTO identity.password_reset_tokens (id, user_id, token, expires_at, created_at, is_used, used_at)
SELECT
    public.seed_uuid('password_reset', i),
    public.seed_uuid('user', ((i - 1) % {p.Users}) + 1),
    'password-reset-token-' || i::text,
    now() + make_interval(hours => 3),
    now() - make_interval(hours => (i % 48)),
    false,
    NULL
FROM generate_series(1, {p.PasswordResetTokens}) AS g(i);

INSERT INTO forum.categories (id, name, description, slug, parent_category_id, post_count, display_order, is_active, created_at, moderator_ids)
SELECT
    public.seed_uuid('category', i),
    'Category ' || lpad(i::text, 3, '0'),
    'Category seeded for load testing #' || i::text,
    'category-' || lpad(i::text, 3, '0'),
    CASE WHEN i > 5 AND i % 5 = 0 THEN public.seed_uuid('category', i - 1) ELSE NULL END,
    0,
    i,
    true,
    now() - make_interval(days => (i % 90)),
    '[]'::jsonb
FROM generate_series(1, {p.Categories}) AS g(i);

INSERT INTO forum.tags (id, name, description, slug, usage_count, created_at)
SELECT
    i,
    'tag_' || lpad(i::text, 4, '0'),
    'Seed tag ' || i::text,
    'tag-' || lpad(i::text, 4, '0'),
    0,
    now() - make_interval(days => (i % 120))
FROM generate_series(1, {p.Tags}) AS g(i);

SELECT setval(pg_get_serial_sequence('forum.tags', 'id'), {p.Tags}, true);

INSERT INTO forum.posts (id, title, content, slug, type, status, author_id, category_id, view_count, vote_score, comment_count, is_pinned, is_locked, created_at, published_at, tags)
SELECT
    public.seed_uuid('post', i),
    'Seed Post #' || i::text,
    repeat('Seed content for post #' || i::text || ' ', 10),
    'seed-post-' || i::text,
    0,
    1,
    public.seed_uuid('user', ((i - 1) % {p.Users}) + 1),
    public.seed_uuid('category', ((i - 1) % {p.Categories}) + 1),
    (i % 2000)::int,
    ((i % 200) - 100)::int,
    0,
    (i % 250 = 0),
    false,
    now() - make_interval(days => (i % 365)),
    now() - make_interval(days => (i % 364)),
    '[]'::jsonb
FROM generate_series(1, {p.Posts}) AS g(i);

INSERT INTO forum.post_tags (post_id, tag_id, added_at)
SELECT
    public.seed_uuid('post', ((i - 1) % {p.Posts}) + 1),
    ((((i - 1) / {p.Posts}) % {p.Tags}) + 1)::int,
    now() - make_interval(days => (i % 180))
FROM generate_series(1, {p.PostTags}) AS g(i);

INSERT INTO forum.comments (id, post_id, author_id, content, parent_comment_id, is_accepted_answer, vote_score, is_deleted, created_at)
SELECT
    public.seed_uuid('comment', i),
    public.seed_uuid('post', ((i - 1) % {p.Posts}) + 1),
    public.seed_uuid('user', ((i - 1) % {p.Users}) + 1),
    'Seed comment #' || i::text,
    CASE WHEN i > 10 AND i % 10 = 0 THEN public.seed_uuid('comment', i - 1) ELSE NULL END,
    (i % 200 = 0),
    ((i % 40) - 20)::int,
    false,
    now() - make_interval(days => (i % 240))
FROM generate_series(1, {p.Comments}) AS g(i);

INSERT INTO forum.post_votes (user_id, post_id, vote_type, created_at)
SELECT
    public.seed_uuid('user', (((i - 1) / {p.Posts}) % {p.Users}) + 1),
    public.seed_uuid('post', ((i - 1) % {p.Posts}) + 1),
    CASE WHEN i % 4 = 0 THEN 2 ELSE 1 END,
    now() - make_interval(days => (i % 120))
FROM generate_series(1, {p.PostVotes}) AS g(i);

INSERT INTO forum.bookmarks (post_id, user_id, created_at)
SELECT
    public.seed_uuid('post', ((i - 1) % {p.Posts}) + 1),
    public.seed_uuid('user', (((i - 1) / {p.Posts}) % {p.Users}) + 1),
    now() - make_interval(days => (i % 180))
FROM generate_series(1, {p.Bookmarks}) AS g(i);

INSERT INTO forum.comment_votes (user_id, comment_id, vote_type, created_at)
SELECT
    public.seed_uuid('user', (((i - 1) / {p.Comments}) % {p.Users}) + 1),
    public.seed_uuid('comment', ((i - 1) % {p.Comments}) + 1),
    CASE WHEN i % 5 = 0 THEN 2 ELSE 1 END,
    now() - make_interval(days => (i % 120))
FROM generate_series(1, {p.CommentVotes}) AS g(i);

INSERT INTO forum.reports (reported_item_id, reported_item_type, reporter_id, reason, description, status, created_at)
SELECT
    CASE WHEN i % 2 = 0 THEN public.seed_uuid('post', ((i - 1) % {p.Posts}) + 1) ELSE public.seed_uuid('comment', ((i - 1) % {p.Comments}) + 1) END,
    CASE WHEN i % 2 = 0 THEN 0 ELSE 1 END,
    public.seed_uuid('user', ((i - 1) % {p.Users}) + 1),
    (i % 6)::int,
    'Auto-generated moderation report #' || i::text,
    CASE WHEN i % 8 = 0 THEN 1 ELSE 0 END,
    now() - make_interval(days => (i % 180))
FROM generate_series(1, {p.Reports}) AS g(i);

INSERT INTO learning.faculties (id, code, name, description, status, manager_id, course_count, created_at, created_by)
SELECT
    public.seed_uuid('faculty', i),
    'FAC' || lpad(i::text, 3, '0'),
    'Faculty ' || i::text,
    'Seed faculty #' || i::text,
    1,
    public.seed_uuid('user', ((i - 1) % {p.Users}) + 1),
    0,
    now() - make_interval(days => (i % 365)),
    public.seed_uuid('user', ((i - 1) % {p.Users}) + 1)
FROM generate_series(1, {p.Faculties}) AS g(i);

INSERT INTO learning.courses (id, code, name, description, semester, status, faculty_id, credits, document_count, enrollment_count, created_at, created_by, moderator_ids)
SELECT
    public.seed_uuid('course', i),
    'COURSE' || lpad(i::text, 5, '0'),
    'Course ' || i::text,
    'Seed course #' || i::text,
    '2026A',
    1,
    public.seed_uuid('faculty', ((i - 1) % {p.Faculties}) + 1),
    ((i - 1) % 5) + 2,
    0,
    ((i - 1) % 500) + 10,
    now() - make_interval(days => (i % 365)),
    public.seed_uuid('user', ((i - 1) % {p.Users}) + 1),
    '[]'::jsonb
FROM generate_series(1, {p.Courses}) AS g(i);

INSERT INTO learning.documents (id, title, description, file_name, file_path, file_size, content_type, file_extension, type, status, uploader_id, course_id, reviewer_id, review_comment, rejection_reason, download_count, view_count, average_rating, rating_count, created_at, submitted_at, reviewed_at)
SELECT
    public.seed_uuid('document', i),
    'Document ' || i::text,
    'Seed learning document #' || i::text,
    'doc-' || i::text || '.pdf',
    '/seed/docs/doc-' || i::text || '.pdf',
    1024 + (i % 2048),
    'application/pdf',
    '.pdf',
    0,
    1,
    public.seed_uuid('user', ((i - 1) % {p.Users}) + 1),
    public.seed_uuid('course', ((i - 1) % {p.Courses}) + 1),
    public.seed_uuid('user', ((i + 13 - 1) % {p.Users}) + 1),
    NULL,
    NULL,
    (i % 500)::int,
    (i % 2000)::int,
    ((i % 50)::numeric / 10.0),
    (i % 300)::int,
    now() - make_interval(days => (i % 365)),
    now() - make_interval(days => (i % 364)),
    now() - make_interval(days => (i % 363))
FROM generate_series(1, {p.Documents}) AS g(i);

INSERT INTO career.companies (id, name, description, industry, size, website, logo_url, founded_year, status, contact_email, registered_by, registered_at, total_job_postings, benefits)
SELECT
    public.seed_uuid('company', i),
    'Company ' || (ARRAY['Tech', 'Solutions', 'Systems', 'Global', 'Innovations', 'Group', 'Networks', 'Labs'])[i % 8 + 1] || ' ' || i::text,
    'Leading provider of ' || (ARRAY['software', 'hardware', 'cloud', 'AI', 'data', 'security', 'fintech', 'edtech'])[i % 8 + 1] || ' solutions.',
    (i % 8)::int,
    (i % 5)::int,
    'https://company' || i::text || '.example.com',
    NULL,
    2000 + (i % 25),
    1,
    'hr+' || i::text || '@company-seed.local',
    public.seed_uuid('user', ((i - 1) % {p.Users}) + 1),
    now() - make_interval(days => (i % 700)),
    0,
    '[]'::jsonb
FROM generate_series(1, {p.Companies}) AS g(i);

INSERT INTO career.job_postings (id, title, description, company_id, posted_by, job_type, experience_level, status, salary_min_amount, salary_max_amount, salary_currency, salary_period, location_city, location_district, location_address, location_is_remote, deadline, created_at, published_at, view_count, application_count, tags)
SELECT
    public.seed_uuid('job', i),
    (ARRAY['Software Engineer', 'Data Scientist', 'Product Manager', 'UX Designer', 'DevOps Engineer', 'Frontend Developer', 'Backend Developer', 'Marketing Specialist'])[i % 8 + 1] || ' ' || i::text,
    'We are looking for a skilled ' || (ARRAY['Software Engineer', 'Data Scientist', 'Product Manager', 'UX Designer', 'DevOps Engineer', 'Frontend Developer', 'Backend Developer', 'Marketing Specialist'])[i % 8 + 1] || ' to join our growing team.',
    public.seed_uuid('company', ((i - 1) % {p.Companies}) + 1),
    public.seed_uuid('user', ((i - 1) % {p.Users}) + 1),
    (i % 4)::int,
    (i % 4)::int,
    1,
    500 + (i % 2000),
    1500 + (i % 5000),
    'USD',
    'MONTH',
    'Ho Chi Minh City',
    'District ' || ((i - 1) % 12 + 1)::text,
    'Seed Address ' || i::text,
    (i % 5 = 0),
    now() + make_interval(days => ((i % 60) + 7)),
    now() - make_interval(days => (i % 120)),
    now() - make_interval(days => (i % 119)),
    (i % 2000)::int,
    0,
    CASE (i % 5)
        WHEN 0 THEN '["IT", "Software"]'::jsonb
        WHEN 1 THEN '["Design", "UX"]'::jsonb
        WHEN 2 THEN '["Marketing", "Sales"]'::jsonb
        WHEN 3 THEN '["Data", "AI"]'::jsonb
        ELSE '["Engineering", "DevOps"]'::jsonb
    END
FROM generate_series(1, {p.JobPostings}) AS g(i);

INSERT INTO career.job_posting_requirements (skill, job_posting_id, is_required)
SELECT
    'skill_' || ((((i - 1) / {p.JobPostings}) % 20) + 1)::text,
    public.seed_uuid('job', ((i - 1) % {p.JobPostings}) + 1),
    true
FROM generate_series(1, {p.JobPostingRequirements}) AS g(i);

INSERT INTO career.applications (id, job_posting_id, applicant_id, status, resume_file_name, resume_file_url, resume_file_size_bytes, resume_content_type, cover_letter_content, submitted_at)
SELECT
    public.seed_uuid('application', i),
    public.seed_uuid('job', ((i - 1) % {p.JobPostings}) + 1),
    public.seed_uuid('user', (((i - 1) / {p.JobPostings}) % {p.Users}) + 1),
    (i % 5)::int,
    'resume-' || i::text || '.pdf',
    'https://cdn.unihub.local/resume-' || i::text || '.pdf',
    1024 + (i % 4096),
    'application/pdf',
    'Cover letter #' || i::text,
    now() - make_interval(days => (i % 180))
FROM generate_series(1, {p.Applications}) AS g(i);

INSERT INTO career.recruiters (id, user_id, company_id, can_manage_job_postings, can_review_applications, can_update_application_status, can_invite_recruiters, status, added_by, added_at)
SELECT
    public.seed_uuid('recruiter', i),
    public.seed_uuid('user', ((i - 1) % {p.Users}) + 1),
    public.seed_uuid('company', ((i - 1) % {p.Companies}) + 1),
    true,
    true,
    true,
    (i % 3 = 0),
    1,
    public.seed_uuid('user', ((i + 7 - 1) % {p.Users}) + 1),
    now() - make_interval(days => (i % 365))
FROM generate_series(1, {p.Recruiters}) AS g(i);

INSERT INTO chat.conversations (id, type, title, created_by, created_at, last_message_at, is_archived, participants)
SELECT
    public.seed_uuid('conversation', i),
    CASE WHEN i % 10 = 0 THEN 1 ELSE 0 END,
    'Conversation ' || i::text,
    public.seed_uuid('user', ((i - 1) % {p.Users}) + 1),
    now() - make_interval(days => (i % 180)),
    now() - make_interval(hours => (i % 72)),
    false,
    '[]'::jsonb
FROM generate_series(1, {p.Conversations}) AS g(i);

INSERT INTO chat.channels (id, name, description, type, owner_id, created_at, is_archived, members, moderators)
SELECT
    public.seed_uuid('channel', i),
    'channel-' || lpad(i::text, 5, '0'),
    'Seed channel #' || i::text,
    0,
    public.seed_uuid('user', ((i - 1) % {p.Users}) + 1),
    now() - make_interval(days => (i % 240)),
    false,
    '[]'::jsonb,
    '[]'::jsonb
FROM generate_series(1, {p.Channels}) AS g(i);

INSERT INTO chat.messages (id, conversation_id, sender_id, content, type, sent_at, edited_at, is_deleted, deleted_at, reply_to_message_id)
SELECT
    public.seed_uuid('message', i),
    public.seed_uuid('conversation', ((i - 1) % {p.Conversations}) + 1),
    public.seed_uuid('user', ((i - 1) % {p.Users}) + 1),
    'Seed message #' || i::text,
    0,
    now() - make_interval(hours => (i % 500)),
    NULL,
    false,
    NULL,
    CASE WHEN i > 20 AND i % 20 = 0 THEN public.seed_uuid('message', i - 1) ELSE NULL END
FROM generate_series(1, {p.Messages}) AS g(i);

INSERT INTO chat.message_attachments (file_name, message_id, file_url, file_size_bytes, mime_type, thumbnail_url)
SELECT
    'attachment-' || i::text || '.png',
    public.seed_uuid('message', ((i - 1) % {p.Messages}) + 1),
    'https://cdn.unihub.local/chat/attachment-' || i::text || '.png',
    2048 + (i % 5000),
    'image/png',
    NULL
FROM generate_series(1, {p.MessageAttachments}) AS g(i);

INSERT INTO chat.message_reactions (user_id, emoji, message_id, reacted_at)
SELECT
    public.seed_uuid('user', (((i - 1) / {p.Messages}) % {p.Users}) + 1),
    CASE (i % 4) WHEN 0 THEN ':thumbsup:' WHEN 1 THEN ':heart:' WHEN 2 THEN ':fire:' ELSE ':tada:' END,
    public.seed_uuid('message', ((i - 1) % {p.Messages}) + 1),
    now() - make_interval(hours => (i % 240))
FROM generate_series(1, {p.MessageReactions}) AS g(i);

INSERT INTO chat.message_read_receipts (user_id, message_id, read_at)
SELECT
    public.seed_uuid('user', (((i - 1) / {p.Messages}) % {p.Users}) + 1),
    public.seed_uuid('message', ((i - 1) % {p.Messages}) + 1),
    now() - make_interval(hours => (i % 180))
FROM generate_series(1, {p.MessageReadReceipts}) AS g(i);

INSERT INTO notification.notification_templates (id, name, display_name, description, category, status, email_subject, email_body, inapp_title, inapp_body, created_by, created_at, channels)
SELECT
    public.seed_uuid('notification_template', i),
    'template_' || lpad(i::text, 3, '0'),
    'Template ' || i::text,
    'Seed notification template #' || i::text,
    (i % 4)::int,
    1,
    'Seed subject ' || i::text,
    'Seed email body for template ' || i::text,
    'Seed in-app title ' || i::text,
    'Seed in-app body ' || i::text,
    public.seed_uuid('user', ((i - 1) % {p.Users}) + 1),
    now() - make_interval(days => (i % 120)),
    '[]'::jsonb
FROM generate_series(1, {p.NotificationTemplates}) AS g(i);

INSERT INTO notification.notification_template_variables (name, template_id, description, example_value)
SELECT
    'var_' || ((((i - 1) / {p.NotificationTemplates}) % 10) + 1)::text,
    public.seed_uuid('notification_template', ((i - 1) % {p.NotificationTemplates}) + 1),
    'Seed variable #' || i::text,
    'example-' || i::text
FROM generate_series(1, {p.NotificationTemplateVariables}) AS g(i);

INSERT INTO notification.notification_preferences (id, user_id, email_enabled, push_enabled, in_app_enabled, created_at)
SELECT
    public.seed_uuid('notification_preference', i),
    public.seed_uuid('user', i),
    true,
    true,
    true,
    now() - make_interval(days => (i % 365))
FROM generate_series(1, {p.NotificationPreferences}) AS g(i);

INSERT INTO notification.notifications (id, recipient_id, template_id, channel, status, content_subject, content_body, content_action_url, content_icon_url, metadata, created_at, sent_at, read_at, dismissed_at, failure_reason, send_attempts)
SELECT
    public.seed_uuid('notification', i),
    public.seed_uuid('user', ((i - 1) % {p.Users}) + 1),
    public.seed_uuid('notification_template', ((i - 1) % {p.NotificationTemplates}) + 1),
    (i % 3)::int,
    CASE WHEN i % 10 = 0 THEN 3 ELSE 1 END,
    'Notification ' || i::text,
    'Seed notification content #' || i::text,
    CASE WHEN i % 7 = 0 THEN 'https://unihub.local/action/' || i::text ELSE NULL END,
    NULL,
    jsonb_build_object('seed', true, 'index', i),
    now() - make_interval(days => (i % 365)),
    now() - make_interval(days => (i % 364)),
    CASE WHEN i % 3 = 0 THEN now() - make_interval(days => (i % 200)) ELSE NULL END,
    CASE WHEN i % 9 = 0 THEN now() - make_interval(days => (i % 150)) ELSE NULL END,
    CASE WHEN i % 10 = 0 THEN 'Simulated delivery issue' ELSE NULL END,
    CASE WHEN i % 10 = 0 THEN 3 ELSE 1 END
FROM generate_series(1, {p.Notifications}) AS g(i);

DROP FUNCTION IF EXISTS public.seed_uuid(text, bigint);
""";
    }

    private sealed record SeedVolumePlan(
        int Users,
        int Permissions,
        int UserRoles,
        int RefreshTokens,
        int PasswordResetTokens,
        int Categories,
        int Tags,
        int Posts,
        int PostTags,
        int PostVotes,
        int Bookmarks,
        int Comments,
        int CommentVotes,
        int Reports,
        int Faculties,
        int Courses,
        int Documents,
        int Companies,
        int JobPostings,
        int JobPostingRequirements,
        int Applications,
        int Recruiters,
        int Conversations,
        int Channels,
        int Messages,
        int MessageAttachments,
        int MessageReactions,
        int MessageReadReceipts,
        int NotificationPreferences,
        int NotificationTemplates,
        int NotificationTemplateVariables,
        int Notifications)
    {
        public int TotalRows =>
            Users + 6 + Permissions + UserRoles + Permissions + RefreshTokens + PasswordResetTokens +
            Categories + Tags + Posts + PostTags + PostVotes + Bookmarks + Comments + CommentVotes + Reports +
            Faculties + Courses + Documents + Companies + JobPostings + JobPostingRequirements + Applications + Recruiters +
            Conversations + Channels + Messages + MessageAttachments + MessageReactions + MessageReadReceipts +
            NotificationPreferences + NotificationTemplates + NotificationTemplateVariables + Notifications;

        public static SeedVolumePlan Create(int targetTotalRows)
        {
            const int baseTotal = 530_290;
            var scale = targetTotalRows <= 0 ? 1.0 : (double)targetTotalRows / baseTotal;

            static int S(int baseCount, double factor) => Math.Max(1, (int)Math.Round(baseCount * factor, MidpointRounding.AwayFromZero));

            var users = S(15_000, scale);
            var posts = S(50_000, scale);
            var comments = S(70_000, scale);
            var messages = S(70_000, scale);
            var notifications = S(65_000, scale);

            return new SeedVolumePlan(
                Users: users,
                Permissions: S(80, scale),
                UserRoles: users,
                RefreshTokens: users,
                PasswordResetTokens: S(3_000, scale),
                Categories: Math.Max(8, S(20, scale)),
                Tags: Math.Max(50, S(400, scale)),
                Posts: posts,
                PostTags: posts,
                PostVotes: S(15_000, scale),
                Bookmarks: S(15_000, scale),
                Comments: comments,
                CommentVotes: S(15_000, scale),
                Reports: S(6_000, scale),
                Faculties: Math.Max(8, S(12, scale)),
                Courses: Math.Max(200, S(2_500, scale)),
                Documents: S(15_000, scale),
                Companies: Math.Max(100, S(1_500, scale)),
                JobPostings: Math.Max(500, S(7_000, scale)),
                JobPostingRequirements: Math.Max(1_000, S(14_000, scale)),
                Applications: S(25_000, scale),
                Recruiters: Math.Max(100, S(1_500, scale)),
                Conversations: S(12_000, scale),
                Channels: Math.Max(200, S(1_200, scale)),
                Messages: messages,
                MessageAttachments: S(7_000, scale),
                MessageReactions: S(12_000, scale),
                MessageReadReceipts: S(12_000, scale),
                NotificationPreferences: users,
                NotificationTemplates: Math.Max(8, S(18, scale)),
                NotificationTemplateVariables: Math.Max(16, S(54, scale)),
                Notifications: notifications);
        }
    }
}
/*
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace UniHub.Infrastructure.Persistence.Seeding;

internal static class HighVolumeDatabaseSeed
{
    public static async Task SeedAsync(ApplicationDbContext context, IConfiguration configuration, ILogger logger)
    {
        var targetTotalRows = configuration.GetValue<int?>("Seeding:HighVolume:TargetTotalRows") ?? 500_000;
        var forceReseed = configuration.GetValue<bool?>("Seeding:HighVolume:ForceReseed") ?? false;

        var existingUsers = await context.Users.CountAsync();
        if (!forceReseed && existingUsers > 0)
        {
            logger.LogInformation(
                "High-volume seed skipped because data already exists ({Count} users). Set Seeding:HighVolume:ForceReseed=true to rebuild dataset.",
                existingUsers);
            return;
        }

        var plan = SeedVolumePlan.Create(targetTotalRows);

        logger.LogInformation(
            "Starting high-volume seed with target ~{TargetRows} rows (plan total: {PlannedRows}).",
            targetTotalRows,
            plan.TotalRows);

        context.Database.SetCommandTimeout(TimeSpan.FromMinutes(30));

        await using var tx = await context.Database.BeginTransactionAsync();
        await context.Database.ExecuteSqlRawAsync(CreateSql(plan));
        await tx.CommitAsync();

        logger.LogInformation(
            "High-volume seed completed. users={Users}, posts={Posts}, comments={Comments}, messages={Messages}, notifications={Notifications}",
            plan.Users,
            plan.Posts,
            plan.Comments,
            plan.Messages,
            plan.Notifications);
    }

    private static string CreateSql(SeedVolumePlan p)
    {
        var sql = new StringBuilder();

        sql.AppendLine("TRUNCATE TABLE");
        sql.AppendLine("\"forum\".\"comment_votes\",");
        sql.AppendLine("\"forum\".\"post_votes\",");
        sql.AppendLine("\"forum\".\"post_tags\",");
        sql.AppendLine("\"forum\".\"bookmarks\",");
        sql.AppendLine("\"forum\".\"comments\",");
        sql.AppendLine("\"forum\".\"reports\",");
        sql.AppendLine("\"forum\".\"posts\",");
        sql.AppendLine("\"forum\".\"tags\",");
        sql.AppendLine("\"forum\".\"categories\",");
        sql.AppendLine("\"chat\".\"message_attachments\",");
        sql.AppendLine("\"chat\".\"message_reactions\",");
        sql.AppendLine("\"chat\".\"message_read_receipts\",");
        sql.AppendLine("\"chat\".\"messages\",");
        sql.AppendLine("\"chat\".\"channels\",");
        sql.AppendLine("\"chat\".\"conversations\",");
        sql.AppendLine("\"career\".\"job_posting_requirements\",");
        sql.AppendLine("\"career\".\"applications\",");
        sql.AppendLine("\"career\".\"recruiters\",");
        sql.AppendLine("\"career\".\"job_postings\",");
        sql.AppendLine("\"career\".\"companies\",");
        sql.AppendLine("\"learning\".\"documents\",");
        sql.AppendLine("\"learning\".\"courses\",");
        sql.AppendLine("\"learning\".\"faculties\",");
        sql.AppendLine("\"notification\".\"notification_template_variables\",");
        sql.AppendLine("\"notification\".\"notifications\",");
        sql.AppendLine("\"notification\".\"notification_preferences\",");
        sql.AppendLine("\"notification\".\"notification_templates\",");
        sql.AppendLine("\"identity\".\"refresh_tokens\",");
        sql.AppendLine("\"identity\".\"password_reset_tokens\",");
        sql.AppendLine("\"identity\".\"user_roles\",");
        sql.AppendLine("\"identity\".\"role_permissions\",");
        sql.AppendLine("\"identity\".\"permissions\",");
        sql.AppendLine("\"identity\".\"roles\",");
        sql.AppendLine("\"identity\".\"users\"");
        sql.AppendLine("RESTART IDENTITY CASCADE;");

        sql.AppendLine(@"
CREATE OR REPLACE FUNCTION public.seed_uuid(seed text, idx bigint)
RETURNS uuid
LANGUAGE SQL
IMMUTABLE
AS $$
SELECT (
    substr(md5(seed || ':' || idx::text), 1, 8) || '-' ||
    substr(md5(seed || ':' || idx::text), 9, 4) || '-' ||
    substr(md5(seed || ':' || idx::text), 13, 4) || '-' ||
    substr(md5(seed || ':' || idx::text), 17, 4) || '-' ||
    substr(md5(seed || ':' || idx::text), 21, 12)
)::uuid;
$$;
");

        sql.AppendLine($@"
INSERT INTO \"identity\".\"users\" (
    \"id\", \"email\", \"password_hash\", \"first_name\", \"last_name\", \"status\", \"created_at\"
)
SELECT
    public.seed_uuid('user', i),
    'user' || lpad(i::text, 6, '0') || '@seed.unihub.local',
    '$2a$12$k312te0PvwsBFoDQ0i9y2ufy5.gzcWlsZDVh5JqVzyrHPgH5bNGbK',
    'User',
    lpad(i::text, 6, '0'),
    1,
    now() - make_interval(days => (i % 365))
FROM generate_series(1, { p.Users}) AS g(i);
");

        sql.AppendLine(@"
INSERT INTO \"identity\".\"roles\" (
    \"id\", \"name\", \"description\", \"is_default\", \"is_system_role\", \"created_at\"
)
VALUES
    (public.seed_uuid('role', 1), 'Admin', 'System administrator role', false, true, now()),
    (public.seed_uuid('role', 2), 'Moderator', 'Forum moderator role', false, true, now()),
    (public.seed_uuid('role', 3), 'Lecturer', 'Lecturer role', false, false, now()),
    (public.seed_uuid('role', 4), 'Student', 'Student role', true, false, now()),
    (public.seed_uuid('role', 5), 'Recruiter', 'Recruiter role', false, false, now()),
    (public.seed_uuid('role', 6), 'Support', 'Support role', false, true, now());
");

        sql.AppendLine($@"
INSERT INTO \"identity\".\"permissions\" (
    \"id\", \"code\", \"name\", \"description\", \"module\", \"resource\", \"action\", \"created_at\"
)
SELECT
    public.seed_uuid('permission', i),
    'seed.module' || ((i - 1) % 12 + 1)::text || '.resource' || ((i - 1) % 18 + 1)::text || '.action' || i::text,
    'Seed Permission ' || i::text,
    'Synthetic permission for load testing',
    'seed_module_' || ((i - 1) % 12 + 1)::text,
    'resource_' || ((i - 1) % 18 + 1)::text,
    'action_' || i::text,
    now()
FROM generate_series(1, { p.Permissions}) AS g(i);
");

        sql.AppendLine($@"
INSERT INTO \"identity\".\"user_roles\" (
    \"id\", \"user_id\", \"role_id\", \"assigned_at\"
)
SELECT
    public.seed_uuid('user_role', i),
    public.seed_uuid('user', i),
    public.seed_uuid('role', ((i - 1) % 4) + 1),
    now() - make_interval(days => (i % 120))
FROM generate_series(1, { p.UserRoles}) AS g(i);
");

        sql.AppendLine($@"
INSERT INTO \"identity\".\"role_permissions\" (
    \"id\", \"role_id\", \"permission_id\", \"scope_type\", \"scope_value\", \"assigned_at\"
)
SELECT
    public.seed_uuid('role_permission', i),
    public.seed_uuid('role', ((i - 1) % 6) + 1),
    public.seed_uuid('permission', i),
    0,
    NULL,
    now() - make_interval(days => (i % 60))
FROM generate_series(1, { p.Permissions}) AS g(i);
");

        sql.AppendLine($@"
INSERT INTO \"identity\".\"refresh_tokens\" (
    \"id\", \"token\", \"expires_at\", \"created_at\", \"created_by_ip\", \"user_id\"
)
SELECT
    public.seed_uuid('refresh_token', i),
    'refresh-token-' || i::text,
    now() + make_interval(days => 14),
    now() - make_interval(hours => (i % 240)),
    '127.0.0.' || ((i - 1) % 200 + 1)::text,
    public.seed_uuid('user', ((i - 1) % {p.Users
}) + 1)
FROM generate_series(1, { p.RefreshTokens}) AS g(i);
");

        sql.AppendLine($@"
INSERT INTO \"identity\".\"password_reset_tokens\" (
    \"id\", \"user_id\", \"token\", \"expires_at\", \"created_at\", \"is_used\", \"used_at\"
)
SELECT
    public.seed_uuid('password_reset', i),
    public.seed_uuid('user', ((i - 1) % {p.Users}) +1),
    'password-reset-token-' || i::text,
    now() + make_interval(hours => 3),
    now() - make_interval(hours => (i % 48)),
    false,
    NULL
FROM generate_series(1, {p.PasswordResetTokens}) AS g(i);
");

        sql.AppendLine($@"
INSERT INTO \"forum\".\"categories\" (
    \"id\", \"name\", \"description\", \"slug\", \"parent_category_id\", \"post_count\", \"display_order\", \"is_active\", \"created_at\", \"moderator_ids\"
)
SELECT
    public.seed_uuid('category', i),
    'Category ' || lpad(i::text, 3, '0'),
    'Category seeded for load testing #' || i::text,
    'category-' || lpad(i::text, 3, '0'),
    CASE WHEN i > 5 AND i % 5 = 0 THEN public.seed_uuid('category', i - 1) ELSE NULL END,
    0,
    i,
    true,
    now() - make_interval(days => (i % 90)),
    '[]'::jsonb
FROM generate_series(1, {p.Categories}) AS g(i);
");

        sql.AppendLine($@"
INSERT INTO \"forum\".\"tags\" (
    \"id\", \"name\", \"description\", \"slug\", \"usage_count\", \"created_at\"
)
SELECT
    i,
    'tag_' || lpad(i::text, 4, '0'),
    'Seed tag ' || i::text,
    'tag-' || lpad(i::text, 4, '0'),
    0,
    now() - make_interval(days => (i % 120))
FROM generate_series(1, {p.Tags}) AS g(i);

SELECT setval(pg_get_serial_sequence('forum.tags', 'id'), { p.Tags}, true);
");

        sql.AppendLine($@"
INSERT INTO \"forum\".\"posts\" (
    \"id\", \"title\", \"content\", \"slug\", \"type\", \"status\", \"author_id\", \"category_id\", \"view_count\", \"vote_score\", \"comment_count\", \"is_pinned\", \"is_locked\", \"created_at\", \"published_at\", \"tags\"
)
SELECT
    public.seed_uuid('post', i),
    'Seed Post #' || i::text,
    repeat('Seed content for post #' || i::text || ' ', 10),
    'seed-post-' || i::text,
    0,
    1,
    public.seed_uuid('user', ((i - 1) % { p.Users}) +1),
    public.seed_uuid('category', ((i - 1) % { p.Categories}) +1),
    (i % 2000)::int,
    ((i % 200) - 100)::int,
    0,
    (i % 250 = 0),
    false,
    now() - make_interval(days => (i % 365)),
    now() - make_interval(days => (i % 365) - 1),
    '[]'::jsonb
FROM generate_series(1, {p.Posts}) AS g(i);
");

        sql.AppendLine($@"
INSERT INTO \"forum\".\"post_tags\" (
    \"post_id\", \"tag_id\", \"added_at\"
)
SELECT
    public.seed_uuid('post', ((i - 1) % { p.Posts}) +1),
    ((((i - 1) / { p.Posts}) % { p.Tags}) +1)::int,
    now() - make_interval(days => (i % 180))
FROM generate_series(1, {p.PostTags}) AS g(i);
");

        sql.AppendLine($@"
INSERT INTO \"forum\".\"comments\" (
    \"id\", \"post_id\", \"author_id\", \"content\", \"parent_comment_id\", \"is_accepted_answer\", \"vote_score\", \"is_deleted\", \"created_at\"
)
SELECT
    public.seed_uuid('comment', i),
    public.seed_uuid('post', ((i - 1) % { p.Posts}) +1),
    public.seed_uuid('user', ((i - 1) % { p.Users}) +1),
    'Seed comment #' || i::text,
    CASE WHEN i > 10 AND i % 10 = 0 THEN public.seed_uuid('comment', i - 1) ELSE NULL END,
    (i % 200 = 0),
    ((i % 40) - 20)::int,
    false,
    now() - make_interval(days => (i % 240))
FROM generate_series(1, {p.Comments}) AS g(i);
");

        sql.AppendLine($@"
INSERT INTO \"forum\".\"post_votes\" (
    \"user_id\", \"post_id\", \"vote_type\", \"created_at\"
)
SELECT
    public.seed_uuid('user', (((i - 1) / { p.Posts}) % { p.Users}) +1),
    public.seed_uuid('post', ((i - 1) % { p.Posts}) +1),
    CASE WHEN i % 4 = 0 THEN 2 ELSE 1 END,
    now() - make_interval(days => (i % 120))
FROM generate_series(1, {p.PostVotes}) AS g(i);
");

        sql.AppendLine($@"
INSERT INTO \"forum\".\"bookmarks\" (
    \"post_id\", \"user_id\", \"created_at\"
)
SELECT
    public.seed_uuid('post', ((i - 1) % { p.Posts}) +1),
    public.seed_uuid('user', (((i - 1) / { p.Posts}) % { p.Users}) +1),
    now() - make_interval(days => (i % 180))
FROM generate_series(1, {p.Bookmarks}) AS g(i);
");

        sql.AppendLine($@"
INSERT INTO \"forum\".\"comment_votes\" (
    \"user_id\", \"comment_id\", \"vote_type\", \"created_at\"
)
SELECT
    public.seed_uuid('user', (((i - 1) / { p.Comments}) % { p.Users}) +1),
    public.seed_uuid('comment', ((i - 1) % { p.Comments}) +1),
    CASE WHEN i % 5 = 0 THEN 2 ELSE 1 END,
    now() - make_interval(days => (i % 120))
FROM generate_series(1, {p.CommentVotes}) AS g(i);
");

        sql.AppendLine($@"
INSERT INTO \"forum\".\"reports\" (
    \"reported_item_id\", \"reported_item_type\", \"reporter_id\", \"reason\", \"description\", \"status\", \"created_at\"
)
SELECT
    CASE WHEN i % 2 = 0 THEN public.seed_uuid('post', ((i - 1) % { p.Posts}) +1) ELSE public.seed_uuid('comment', ((i - 1) % { p.Comments}) +1) END,
    CASE WHEN i % 2 = 0 THEN 0 ELSE 1 END,
    public.seed_uuid('user', ((i - 1) % { p.Users}) +1),
    (i % 6)::int,
    'Auto-generated moderation report #' || i::text,
    CASE WHEN i % 8 = 0 THEN 1 ELSE 0 END,
    now() - make_interval(days => (i % 180))
FROM generate_series(1, {p.Reports}) AS g(i);
");

        sql.AppendLine($@"
INSERT INTO \"learning\".\"faculties\" (
    \"id\", \"code\", \"name\", \"description\", \"status\", \"manager_id\", \"course_count\", \"created_at\", \"created_by\"
)
SELECT
    public.seed_uuid('faculty', i),
    'FAC' || lpad(i::text, 3, '0'),
    'Faculty ' || i::text,
    'Seed faculty #' || i::text,
    1,
    public.seed_uuid('user', ((i - 1) % { p.Users}) +1),
    0,
    now() - make_interval(days => (i % 365)),
    public.seed_uuid('user', ((i - 1) % { p.Users}) +1)
FROM generate_series(1, {p.Faculties}) AS g(i);
");

        sql.AppendLine($@"
INSERT INTO \"learning\".\"courses\" (
    \"id\", \"code\", \"name\", \"description\", \"semester\", \"status\", \"faculty_id\", \"credits\", \"document_count\", \"enrollment_count\", \"created_at\", \"created_by\", \"moderator_ids\"
)
SELECT
    public.seed_uuid('course', i),
    'COURSE' || lpad(i::text, 5, '0'),
    'Course ' || i::text,
    'Seed course #' || i::text,
    '2026A',
    1,
    public.seed_uuid('faculty', ((i - 1) % { p.Faculties}) +1),
    ((i - 1) % 5) + 2,
    0,
    ((i - 1) % 500) + 10,
    now() - make_interval(days => (i % 365)),
    public.seed_uuid('user', ((i - 1) % { p.Users}) +1),
    '[]'::jsonb
FROM generate_series(1, {p.Courses}) AS g(i);
");

        sql.AppendLine($@"
INSERT INTO \"learning\".\"documents\" (
    \"id\", \"title\", \"description\", \"file_name\", \"file_path\", \"file_size\", \"content_type\", \"file_extension\", \"type\", \"status\", \"uploader_id\", \"course_id\", \"reviewer_id\", \"review_comment\", \"rejection_reason\", \"download_count\", \"view_count\", \"average_rating\", \"rating_count\", \"created_at\", \"submitted_at\", \"reviewed_at\"
)
SELECT
    public.seed_uuid('document', i),
    'Document ' || i::text,
    'Seed learning document #' || i::text,
    'doc-' || i::text || '.pdf',
    '/seed/docs/doc-' || i::text || '.pdf',
    1024 + (i % 2048),
    'application/pdf',
    '.pdf',
    0,
    1,
    public.seed_uuid('user', ((i - 1) % { p.Users}) +1),
    public.seed_uuid('course', ((i - 1) % { p.Courses}) +1),
    public.seed_uuid('user', ((i + 13 - 1) % { p.Users}) +1),
    NULL,
    NULL,
    (i % 500)::int,
    (i % 2000)::int,
    ((i % 50)::numeric / 10.0),
    (i % 300)::int,
    now() - make_interval(days => (i % 365)),
    now() - make_interval(days => (i % 364)),
    now() - make_interval(days => (i % 363))
FROM generate_series(1, {p.Documents}) AS g(i);
");

        sql.AppendLine($@"
INSERT INTO \"career\".\"companies\" (
    \"id\", \"name\", \"description\", \"industry\", \"size\", \"website\", \"logo_url\", \"founded_year\", \"status\", \"contact_email\", \"registered_by\", \"registered_at\", \"total_job_postings\", \"benefits\"
)
SELECT
    public.seed_uuid('company', i),
    'Company ' || (ARRAY['Tech', 'Solutions', 'Systems', 'Global', 'Innovations', 'Group', 'Networks', 'Labs'])[i % 8 + 1] || ' ' || i::text,
    'Leading provider of ' || (ARRAY['software', 'hardware', 'cloud', 'AI', 'data', 'security', 'fintech', 'edtech'])[i % 8 + 1] || ' solutions.',
    (i % 8)::int,
    (i % 5)::int,
    'https://company' || i::text || '.example.com',
    NULL,
    2000 + (i % 25),
    1,
    'hr+' || i::text || '@company-seed.local',
    public.seed_uuid('user', ((i - 1) % { p.Users}) +1),
    now() - make_interval(days => (i % 700)),
    0,
    '[]'::jsonb
FROM generate_series(1, {p.Companies}) AS g(i);
");

        sql.AppendLine($@"
INSERT INTO \"career\".\"job_postings\" (
    \"id\", \"title\", \"description\", \"company_id\", \"posted_by\", \"job_type\", \"experience_level\", \"status\", \"salary_min_amount\", \"salary_max_amount\", \"salary_currency\", \"salary_period\", \"location_city\", \"location_district\", \"location_address\", \"location_is_remote\", \"deadline\", \"created_at\", \"published_at\", \"view_count\", \"application_count\", \"tags\"
)
SELECT
    public.seed_uuid('job', i),
    (ARRAY['Software Engineer', 'Data Scientist', 'Product Manager', 'UX Designer', 'DevOps Engineer', 'Frontend Developer', 'Backend Developer', 'Marketing Specialist'])[i % 8 + 1] || ' ' || i::text,
    'We are looking for a skilled ' || (ARRAY['Software Engineer', 'Data Scientist', 'Product Manager', 'UX Designer', 'DevOps Engineer', 'Frontend Developer', 'Backend Developer', 'Marketing Specialist'])[i % 8 + 1] || ' to join our growing team.',
    public.seed_uuid('company', ((i - 1) % { p.Companies}) +1),
    public.seed_uuid('user', ((i - 1) % { p.Users}) +1),
    (i % 4)::int,
    (i % 4)::int,
    1,
    500 + (i % 2000),
    1500 + (i % 5000),
    'USD',
    'MONTH',
    'Ho Chi Minh City',
    'District ' || ((i - 1) % 12 + 1)::text,
    'Seed Address ' || i::text,
    (i % 5 = 0),
    now() + make_interval(days => ((i % 60) + 7)),
    now() - make_interval(days => (i % 120)),
    now() - make_interval(days => (i % 119)),
    (i % 2000)::int,
    0,
    CASE (i % 5)
        WHEN 0 THEN '[\"IT\", \"Software\"]'::jsonb
        WHEN 1 THEN '[\"Design\", \"UX\"]'::jsonb
        WHEN 2 THEN '[\"Marketing\", \"Sales\"]'::jsonb
        WHEN 3 THEN '[\"Data\", \"AI\"]'::jsonb
        ELSE '[\"Engineering\", \"DevOps\"]'::jsonb
    END
FROM generate_series(1, {p.JobPostings}) AS g(i);
");

        sql.AppendLine($@"
INSERT INTO \"career\".\"job_posting_requirements\" (
    \"skill\", \"job_posting_id\", \"is_required\"
)
SELECT
    'skill_' || ((((i - 1) / {p.JobPostings}) % 20) +1)::text,
    public.seed_uuid('job', ((i - 1) % { p.JobPostings}) +1),
    true
FROM generate_series(1, {p.JobPostingRequirements}) AS g(i);
");

        sql.AppendLine($@"
INSERT INTO \"career\".\"applications\" (
    \"id\", \"job_posting_id\", \"applicant_id\", \"status\", \"resume_file_name\", \"resume_file_url\", \"resume_file_size_bytes\", \"resume_content_type\", \"cover_letter_content\", \"submitted_at\"
)
SELECT
    public.seed_uuid('application', i),
    public.seed_uuid('job', ((i - 1) % { p.JobPostings}) +1),
    public.seed_uuid('user', (((i - 1) / { p.JobPostings}) % { p.Users}) +1),
    (i % 5)::int,
    'resume-' || i::text || '.pdf',
    'https://cdn.unihub.local/resume-' || i::text || '.pdf',
    1024 + (i % 4096),
    'application/pdf',
    'Cover letter #' || i::text,
    now() - make_interval(days => (i % 180))
FROM generate_series(1, {p.Applications}) AS g(i);
");

        sql.AppendLine($@"
INSERT INTO \"career\".\"recruiters\" (
    \"id\", \"user_id\", \"company_id\", \"can_manage_job_postings\", \"can_review_applications\", \"can_update_application_status\", \"can_invite_recruiters\", \"status\", \"added_by\", \"added_at\"
)
SELECT
    public.seed_uuid('recruiter', i),
    public.seed_uuid('user', ((i - 1) % { p.Users}) +1),
    public.seed_uuid('company', ((i - 1) % { p.Companies}) +1),
    true,
    true,
    true,
    (i % 3 = 0),
    1,
    public.seed_uuid('user', ((i + 7 - 1) % { p.Users}) +1),
    now() - make_interval(days => (i % 365))
FROM generate_series(1, {p.Recruiters}) AS g(i);
");

        sql.AppendLine($@"
INSERT INTO \"chat\".\"conversations\" (
    \"id\", \"type\", \"title\", \"created_by\", \"created_at\", \"last_message_at\", \"is_archived\", \"participants\"
)
SELECT
    public.seed_uuid('conversation', i),
    CASE WHEN i % 10 = 0 THEN 1 ELSE 0 END,
    'Conversation ' || i::text,
    public.seed_uuid('user', ((i - 1) % { p.Users}) +1),
    now() - make_interval(days => (i % 180)),
    now() - make_interval(hours => (i % 72)),
    false,
    '[]'::jsonb
FROM generate_series(1, {p.Conversations}) AS g(i);
");

        sql.AppendLine($@"
INSERT INTO \"chat\".\"channels\" (
    \"id\", \"name\", \"description\", \"type\", \"owner_id\", \"created_at\", \"is_archived\", \"members\", \"moderators\"
)
SELECT
    public.seed_uuid('channel', i),
    'channel-' || lpad(i::text, 5, '0'),
    'Seed channel #' || i::text,
    0,
    public.seed_uuid('user', ((i - 1) % { p.Users}) +1),
    now() - make_interval(days => (i % 240)),
    false,
    '[]'::jsonb,
    '[]'::jsonb
FROM generate_series(1, {p.Channels}) AS g(i);
");

        sql.AppendLine($@"
INSERT INTO \"chat\".\"messages\" (
    \"id\", \"conversation_id\", \"sender_id\", \"content\", \"type\", \"sent_at\", \"edited_at\", \"is_deleted\", \"deleted_at\", \"reply_to_message_id\"
)
SELECT
    public.seed_uuid('message', i),
    public.seed_uuid('conversation', ((i - 1) % { p.Conversations}) +1),
    public.seed_uuid('user', ((i - 1) % { p.Users}) +1),
    'Seed message #' || i::text,
    0,
    now() - make_interval(hours => (i % 500)),
    NULL,
    false,
    NULL,
    CASE WHEN i > 20 AND i % 20 = 0 THEN public.seed_uuid('message', i - 1) ELSE NULL END
FROM generate_series(1, {p.Messages}) AS g(i);
");

        sql.AppendLine($@"
INSERT INTO \"chat\".\"message_attachments\" (
    \"file_name\", \"message_id\", \"file_url\", \"file_size_bytes\", \"mime_type\", \"thumbnail_url\"
)
SELECT
    'attachment-' || i::text || '.png',
    public.seed_uuid('message', ((i - 1) % { p.Messages}) +1),
    'https://cdn.unihub.local/chat/attachment-' || i::text || '.png',
    2048 + (i % 5000),
    'image/png',
    NULL
FROM generate_series(1, {p.MessageAttachments}) AS g(i);
");

        sql.AppendLine($@"
INSERT INTO \"chat\".\"message_reactions\" (
    \"user_id\", \"emoji\", \"message_id\", \"reacted_at\"
)
SELECT
    public.seed_uuid('user', (((i - 1) / { p.Messages}) % { p.Users}) +1),
    CASE(i % 4) WHEN 0 THEN '👍' WHEN 1 THEN '❤️' WHEN 2 THEN '🔥' ELSE '🎉' END,
    public.seed_uuid('message', ((i - 1) % { p.Messages}) +1),
    now() - make_interval(hours => (i % 240))
FROM generate_series(1, {p.MessageReactions}) AS g(i);
");

        sql.AppendLine($@"
INSERT INTO \"chat\".\"message_read_receipts\" (
    \"user_id\", \"message_id\", \"read_at\"
)
SELECT
    public.seed_uuid('user', (((i - 1) / { p.Messages}) % { p.Users}) +1),
    public.seed_uuid('message', ((i - 1) % { p.Messages}) +1),
    now() - make_interval(hours => (i % 180))
FROM generate_series(1, {p.MessageReadReceipts}) AS g(i);
");

        sql.AppendLine($@"
INSERT INTO \"notification\".\"notification_templates\" (
    \"id\", \"name\", \"display_name\", \"description\", \"category\", \"status\", \"email_subject\", \"email_body\", \"inapp_title\", \"inapp_body\", \"created_by\", \"created_at\", \"channels\"
)
SELECT
    public.seed_uuid('notification_template', i),
    'template_' || lpad(i::text, 3, '0'),
    'Template ' || i::text,
    'Seed notification template #' || i::text,
    (i % 4)::int,
    1,
    'Seed subject ' || i::text,
    'Seed email body for template ' || i::text,
    'Seed in-app title ' || i::text,
    'Seed in-app body ' || i::text,
    public.seed_uuid('user', ((i - 1) % { p.Users}) +1),
    now() - make_interval(days => (i % 120)),
    '[]'::jsonb
FROM generate_series(1, {p.NotificationTemplates}) AS g(i);
");

        sql.AppendLine($@"
INSERT INTO \"notification\".\"notification_template_variables\" (
    \"name\", \"template_id\", \"description\", \"example_value\"
)
SELECT
    'var_' || ((((i - 1) / {p.NotificationTemplates}) % 10) +1)::text,
    public.seed_uuid('notification_template', ((i - 1) % { p.NotificationTemplates}) +1),
    'Seed variable #' || i::text,
    'example-' || i::text
FROM generate_series(1, {p.NotificationTemplateVariables}) AS g(i);
");

        sql.AppendLine($@"
INSERT INTO \"notification\".\"notification_preferences\" (
    \"id\", \"user_id\", \"email_enabled\", \"push_enabled\", \"in_app_enabled\", \"created_at\"
)
SELECT
    public.seed_uuid('notification_preference', i),
    public.seed_uuid('user', i),
    true,
    true,
    true,
    now() - make_interval(days => (i % 365))
FROM generate_series(1, {p.NotificationPreferences}) AS g(i);
");

        sql.AppendLine($@"
INSERT INTO \"notification\".\"notifications\" (
    \"id\", \"recipient_id\", \"template_id\", \"channel\", \"status\", \"content_subject\", \"content_body\", \"content_action_url\", \"content_icon_url\", \"metadata\", \"created_at\", \"sent_at\", \"read_at\", \"dismissed_at\", \"failure_reason\", \"send_attempts\"
)
SELECT
    public.seed_uuid('notification', i),
    public.seed_uuid('user', ((i - 1) % { p.Users}) +1),
    public.seed_uuid('notification_template', ((i - 1) % { p.NotificationTemplates}) +1),
    (i % 3)::int,
    CASE WHEN i % 10 = 0 THEN 3 ELSE 1 END,
    'Notification ' || i::text,
    'Seed notification content #' || i::text,
    CASE WHEN i % 7 = 0 THEN 'https://unihub.local/action/' || i::text ELSE NULL END,
    NULL,
    jsonb_build_object('seed', true, 'index', i),
    now() - make_interval(days => (i % 365)),
    now() - make_interval(days => (i % 365) - 1),
    CASE WHEN i % 3 = 0 THEN now() - make_interval(days => (i % 200)) ELSE NULL END,
    CASE WHEN i % 9 = 0 THEN now() - make_interval(days => (i % 150)) ELSE NULL END,
    CASE WHEN i % 10 = 0 THEN 'Simulated delivery issue' ELSE NULL END,
    CASE WHEN i % 10 = 0 THEN 3 ELSE 1 END
FROM generate_series(1, {p.Notifications}) AS g(i);
");

        sql.AppendLine("DROP FUNCTION IF EXISTS public.seed_uuid(text, bigint);");

return sql.ToString();
    }

    private sealed record SeedVolumePlan(
        int Users,
        int Roles,
        int Permissions,
        int UserRoles,
        int RolePermissions,
        int RefreshTokens,
        int PasswordResetTokens,
        int Categories,
        int Tags,
        int Posts,
        int PostTags,
        int PostVotes,
        int Bookmarks,
        int Comments,
        int CommentVotes,
        int Reports,
        int Faculties,
        int Courses,
        int Documents,
        int Companies,
        int JobPostings,
        int JobPostingRequirements,
        int Applications,
        int Recruiters,
        int Conversations,
        int Channels,
        int Messages,
        int MessageAttachments,
        int MessageReactions,
        int MessageReadReceipts,
        int NotificationPreferences,
        int NotificationTemplates,
        int NotificationTemplateVariables,
        int Notifications)
{
    public int TotalRows =>
        Users + Roles + Permissions + UserRoles + RolePermissions + RefreshTokens + PasswordResetTokens +
        Categories + Tags + Posts + PostTags + PostVotes + Bookmarks + Comments + CommentVotes + Reports +
        Faculties + Courses + Documents + Companies + JobPostings + JobPostingRequirements + Applications + Recruiters +
        Conversations + Channels + Messages + MessageAttachments + MessageReactions + MessageReadReceipts +
        NotificationPreferences + NotificationTemplates + NotificationTemplateVariables + Notifications;

    public static SeedVolumePlan Create(int targetTotalRows)
    {
        const int baseTotal = 530_290;
        var scale = targetTotalRows <= 0 ? 1.0 : (double)targetTotalRows / baseTotal;

        static int S(int baseCount, double factor) => Math.Max(1, (int)Math.Round(baseCount * factor, MidpointRounding.AwayFromZero));

        var users = S(15_000, scale);
        var posts = S(50_000, scale);
        var comments = S(70_000, scale);
        var messages = S(70_000, scale);
        var notifications = S(65_000, scale);

        return new SeedVolumePlan(
            Users: users,
            Roles: 6,
            Permissions: S(80, scale),
            UserRoles: users,
            RolePermissions: S(80, scale),
            RefreshTokens: users,
            PasswordResetTokens: S(3_000, scale),
            Categories: Math.Max(8, S(20, scale)),
            Tags: Math.Max(50, S(400, scale)),
            Posts: posts,
            PostTags: posts,
            PostVotes: S(15_000, scale),
            Bookmarks: S(15_000, scale),
            Comments: comments,
            CommentVotes: S(15_000, scale),
            Reports: S(6_000, scale),
            Faculties: Math.Max(8, S(12, scale)),
            Courses: Math.Max(200, S(2_500, scale)),
            Documents: S(15_000, scale),
            Companies: Math.Max(100, S(1_500, scale)),
            JobPostings: Math.Max(500, S(7_000, scale)),
            JobPostingRequirements: Math.Max(1_000, S(14_000, scale)),
            Applications: S(25_000, scale),
            Recruiters: Math.Max(100, S(1_500, scale)),
            Conversations: S(12_000, scale),
            Channels: Math.Max(200, S(1_200, scale)),
            Messages: messages,
            MessageAttachments: S(7_000, scale),
            MessageReactions: S(12_000, scale),
            MessageReadReceipts: S(12_000, scale),
            NotificationPreferences: users,
            NotificationTemplates: Math.Max(8, S(18, scale)),
            NotificationTemplateVariables: Math.Max(16, S(54, scale)),
            Notifications: notifications);
    }
    }
    }
    */
