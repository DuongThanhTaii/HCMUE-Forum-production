using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace UniHub.Infrastructure.Persistence.Seeding;

/// <summary>
/// Seeds initial role-permission assignments for all default roles.
/// Must run AFTER IdentitySeed (permissions and roles must already exist).
/// Idempotent: upserts required role-permission rows without duplicating existing mappings.
/// Uses raw SQL INSERT to bypass EF owned-type tracking complexity (PermissionScope.Type
/// has private set; which causes scope_type = null when using context.Add() on the aggregate).
/// </summary>
internal static class RolePermissionSeeder
{
    // Permission codes per role
    private static readonly string[] ModeratorCodes =
    [
        "forum.thread_channels.manage",
        "forum.reports.review",
        "forum.posts.read",
        "forum.posts.update",
        "forum.posts.delete",
        "forum.comments.read",
        "forum.comments.update",
        "forum.comments.delete",
        "forum.categories.read",
        "forum.tags.read",
        "learning.documents.read",
        "learning.documents.approve",
        "learning.documents.reject",
        "learning.documents.request_revision",
    ];

    private static readonly string[] LecturerCodes =
    [
        "forum.posts.read",
        "forum.posts.create",
        "forum.posts.update",
        "forum.comments.read",
        "forum.comments.create",
        "forum.comments.update",
        "forum.comments.delete",
        "learning.courses.read",
        "learning.courses.create",
        "learning.courses.update",
        "learning.courses.delete",
        "learning.documents.read",
        "learning.documents.create",
        "learning.documents.update",
        "learning.documents.delete",
        "learning.faculties.read",
        "notification.notifications.read",
    ];

    private static readonly string[] StudentCodes =
    [
        "forum.posts.read",
        "forum.posts.create",
        "forum.comments.read",
        "forum.comments.create",
        "learning.courses.read",
        "learning.documents.read",
        "notification.notifications.read",
        "chat.channels.read",
        "chat.channels.create",
        "career.companies.read",
        "career.jobpostings.read",
    ];

    private static readonly string[] RecruiterCodes =
    [
        "career.companies.read",
        "career.companies.create",
        "career.companies.update",
        "career.jobpostings.read",
        "career.jobpostings.create",
        "career.jobpostings.update",
        "notification.notifications.read",
    ];

    public static async Task SeedAsync(ApplicationDbContext context, ILogger logger)
    {
        // Load roles and permissions by name/code (read-only, no tracking)
        var roles = await context.Roles.AsNoTracking()
            .Select(r => new { r.Id, r.Name })
            .ToListAsync();

        var permissions = await context.Permissions.AsNoTracking()
            .Select(p => new { p.Id, p.Code })
            .ToListAsync();

        if (roles.Count == 0 || permissions.Count == 0)
        {
            logger.LogWarning("Roles or permissions not found. Run IdentitySeed first. Skipping.");
            return;
        }

        var roleByName = roles.ToDictionary(r => r.Name, r => r.Id, StringComparer.Ordinal);
        var permByCode = permissions.ToDictionary(p => p.Code, p => p.Id, StringComparer.OrdinalIgnoreCase);

        // Global scope type = 1 (PermissionScopeType.Global)
        const int globalScopeType = 1;
        var now = DateTime.UtcNow;
        var rows = new List<(Guid Id, Guid RoleId, Guid PermId)>();

        // Admin → all permissions
        if (roleByName.TryGetValue("Admin", out var adminRoleId))
        {
            foreach (var perm in permissions)
                rows.Add((Guid.NewGuid(), adminRoleId.Value, perm.Id.Value));
        }

        AddCodedRows(rows, roleByName, permByCode, "Moderator", ModeratorCodes, logger);
        AddCodedRows(rows, roleByName, permByCode, "Lecturer", LecturerCodes, logger);
        AddCodedRows(rows, roleByName, permByCode, "Recruiter", RecruiterCodes, logger);
        AddCodedRows(rows, roleByName, permByCode, "Student", StudentCodes, logger);

        if (rows.Count == 0)
        {
            logger.LogWarning("No role-permission rows to insert.");
            return;
        }

        // Batch-insert with explicit NOT EXISTS guard (does not depend on unique constraints)
        const int batchSize = 50;
        var totalInserted = 0;

        for (var i = 0; i < rows.Count; i += batchSize)
        {
            var batch = rows.Skip(i).Take(batchSize).ToList();
            var selectClauses = batch.Select(row =>
                $"""
                SELECT '{row.Id}'::uuid, '{row.RoleId}'::uuid, '{row.PermId}'::uuid, {globalScopeType}, NULL::text, '{now:yyyy-MM-dd HH:mm:ss.ffffff}'::timestamp
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM identity.role_permissions rp
                    WHERE rp.role_id = '{row.RoleId}'::uuid
                      AND rp.permission_id = '{row.PermId}'::uuid
                      AND rp.scope_type = {globalScopeType}
                      AND rp.scope_value IS NULL
                )
                """);

            var sql = $"""
                INSERT INTO identity.role_permissions (id, role_id, permission_id, scope_type, scope_value, assigned_at)
                {string.Join("\nUNION ALL\n", selectClauses)};
                """;

            await context.Database.ExecuteSqlRawAsync(sql);
            totalInserted += batch.Count;
        }

        logger.LogInformation("RolePermissionSeeder: seeded {Total} role-permission assignments.", totalInserted);
    }

    private static void AddCodedRows(
        List<(Guid Id, Guid RoleId, Guid PermId)> rows,
        Dictionary<string, UniHub.Identity.Domain.Roles.RoleId> roleByName,
        Dictionary<string, UniHub.Identity.Domain.Permissions.PermissionId> permByCode,
        string roleName,
        string[] codes,
        ILogger logger)
    {
        if (!roleByName.TryGetValue(roleName, out var roleId))
        {
            logger.LogWarning("Role '{RoleName}' not found. Skipping.", roleName);
            return;
        }

        var count = 0;
        foreach (var code in codes)
        {
            if (!permByCode.TryGetValue(code, out var permId))
            {
                logger.LogWarning("Permission '{Code}' not found for role '{Role}'. Skipping.", code, roleName);
                continue;
            }
            rows.Add((Guid.NewGuid(), roleId.Value, permId.Value));
            count++;
        }

        logger.LogInformation("Prepared {Count} permissions for {RoleName} role.", count, roleName);
    }
}
