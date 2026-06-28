using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UniHub.Identity.Domain.Permissions;
using UniHub.Identity.Domain.Roles;
using UniHub.Identity.Domain.Users;
using UniHub.Identity.Domain.Users.ValueObjects;

namespace UniHub.Infrastructure.Persistence.Seeding;

/// <summary>
/// Seeds identity data: permissions, roles, admin user, and idempotent demo accounts.
/// </summary>
internal static class IdentitySeed
{
    /// <summary>Pre-computed BCrypt hash for "Admin@123456" (workFactor: 12). Shared by seeded dev accounts.</summary>
    private const string DevSeedPasswordHash =
        "$2a$12$k312te0PvwsBFoDQ0i9y2ufy5.gzcWlsZDVh5JqVzyrHPgH5bNGbK";

    /// <summary>
    /// Fixed well-known ID for the seeded admin user.
    /// Must match DefaultActorId used by LearningBulkSeed so uploaderName resolves correctly.
    /// </summary>
    internal static readonly Guid AdminSeedId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public static async Task SeedAsync(ApplicationDbContext context, ILogger logger)
    {
        await EnsureMissingPermissionsAsync(context, logger);
        await EnsureCoreRolesAsync(context, logger);
        await EnsureAdminUserAsync(context, logger);

        await EnsureDemoAccountsAsync(context, logger);
    }

    private static async Task EnsureCoreRolesAsync(ApplicationDbContext context, ILogger logger)
    {
        var roleSpecs = new (string Name, string Description, bool IsDefault)[]
        {
            ("Admin", "System Administrator with full access", false),
            ("Moderator", "Forum and content moderator", false),
            ("Lecturer", "University lecturer with course management", false),
            ("Recruiter", "Recruiter role for enterprise hiring workflows", false),
            ("Student", "Regular student user", true)
        };

        var existingRoleNames = await context.Roles
            .AsNoTracking()
            .Select(r => r.Name)
            .ToListAsync();
        var existing = new HashSet<string>(existingRoleNames, StringComparer.Ordinal);

        var toAdd = new List<Role>();
        foreach (var spec in roleSpecs)
        {
            if (existing.Contains(spec.Name))
            {
                continue;
            }

            var role = Role.Create(spec.Name, spec.Description, spec.IsDefault).Value;
            toAdd.Add(role);
        }

        if (toAdd.Count == 0)
        {
            logger.LogInformation("Identity roles already present.");
            return;
        }

        context.Roles.AddRange(toAdd);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} missing roles.", toAdd.Count);
    }

    private static async Task EnsureAdminUserAsync(ApplicationDbContext context, ILogger logger)
    {
        var adminEmail = Email.Create("admin@unihub.edu.vn").Value;
        var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
        if (adminRole is null)
        {
            logger.LogWarning("Admin role not found, cannot ensure admin user.");
            return;
        }

        var existingAdmin = await context.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
        if (existingAdmin is null)
        {
            var adminProfile = UserProfile.Create("Admin", "UniHub").Value;
            var adminUser = User.CreateWithId(
                UserId.Create(AdminSeedId),
                adminEmail,
                DevSeedPasswordHash,
                adminProfile).Value;

            var assignRole = adminUser.AssignRole(adminRole.Id);
            if (assignRole.IsFailure)
            {
                throw new InvalidOperationException(
                    $"Failed to assign Admin role to seeded admin user: {assignRole.Error.Message}");
            }

            context.Users.Add(adminUser);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded admin user with Admin role: admin@unihub.edu.vn");
            return;
        }

        var hasAdminRole = await context.UserRoles.AnyAsync(ur => ur.UserId == existingAdmin.Id && ur.RoleId == adminRole.Id);
        if (!hasAdminRole)
        {
            existingAdmin.AssignRole(adminRole.Id);
            await context.SaveChangesAsync();
            logger.LogInformation("Backfilled Admin role for admin@unihub.edu.vn.");
        }
    }

    private static async Task EnsureMissingPermissionsAsync(ApplicationDbContext context, ILogger logger)
    {
        var existingCodes = await context.Permissions
            .AsNoTracking()
            .Select(p => p.Code)
            .ToListAsync();

        var existingSet = new HashSet<string>(existingCodes, StringComparer.OrdinalIgnoreCase);
        var all = CreatePermissions();
        var missing = all.Where(p => !existingSet.Contains(p.Code)).ToList();
        if (missing.Count == 0)
        {
            return;
        }

        context.Permissions.AddRange(missing);
        await context.SaveChangesAsync();
        logger.LogInformation("Backfilled {Count} missing permissions.", missing.Count);
    }

    /// <summary>
    /// Adds moderator / lecturer / student demo logins if missing (safe on existing databases).
    /// Password for all: Admin@123456 (same as seeded admin).
    /// </summary>
    private static async Task EnsureDemoAccountsAsync(ApplicationDbContext context, ILogger logger)
    {
        if (!await context.Roles.AnyAsync())
        {
            return;
        }

        var roles = await context.Roles.AsNoTracking().ToListAsync();
        var roleByName = roles.ToDictionary(r => r.Name, StringComparer.Ordinal);

        var demoAccounts = new (string Email, string FirstName, string LastName, string RoleName)[]
        {
            ("moderator@unihub.edu.vn", "Moderation", "Demo", "Moderator"),
            ("lecturer@unihub.edu.vn", "Lecturer", "Demo", "Lecturer"),
            ("recruiter@unihub.edu.vn", "Recruiter", "Demo", "Recruiter"),
            ("bosch@unihub.edu.vn", "Bosch", "Recruiter", "Recruiter"),
            ("nab@unihub.edu.vn", "NAB", "Recruiter", "Recruiter"),
            ("sap@unihub.edu.vn", "SAP", "Recruiter", "Recruiter"),
            ("student@unihub.edu.vn", "Student", "Demo", "Student"),
            ("student2@unihub.edu.vn", "Sinh viên", "Hai", "Student"),
            ("student3@unihub.edu.vn", "Sinh viên", "Ba", "Student"),
            ("forum.test1@unihub.edu.vn", "Forum", "Tester One", "Student"),
        };

        foreach (var (email, firstName, lastName, roleName) in demoAccounts)
        {
            var emailResult = Email.Create(email);
            if (emailResult.IsFailure)
            {
                continue;
            }

            var emailVo = emailResult.Value;
            if (await context.Users.AnyAsync(u => u.Email == emailVo))
            {
                continue;
            }

            if (!roleByName.TryGetValue(roleName, out var role))
            {
                logger.LogWarning("Demo user {Email} skipped: role {RoleName} not found.", email, roleName);
                continue;
            }

            var profileResult = UserProfile.Create(firstName, lastName);
            if (profileResult.IsFailure)
            {
                logger.LogWarning("Demo user {Email} skipped: invalid profile.", email);
                continue;
            }

            var userResult = User.Create(emailVo, DevSeedPasswordHash, profileResult.Value);
            if (userResult.IsFailure)
            {
                logger.LogWarning("Demo user {Email} skipped: {Message}", email, userResult.Error.Message);
                continue;
            }

            var assign = userResult.Value.AssignRole(role.Id);
            if (assign.IsFailure)
            {
                throw new InvalidOperationException(
                    $"Failed to assign role {roleName} to demo user {email}: {assign.Error.Message}");
            }

            context.Users.Add(userResult.Value);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded demo user {Email} with role {RoleName}.", email, roleName);
        }
    }

    private static List<Permission> CreatePermissions()
    {
        // Permission code format: {module}.{resource}.{action}
        var permissionData = new[]
        {
            ("identity.users.read", "Read users"),
            ("identity.users.create", "Create users"),
            ("identity.users.update", "Update users"),
            ("identity.users.delete", "Delete users"),
            ("identity.roles.read", "Read roles"),
            ("identity.roles.create", "Create roles"),
            ("identity.roles.update", "Update roles"),
            ("identity.roles.delete", "Delete roles"),
            ("forum.posts.read", "Read posts"),
            ("forum.posts.create", "Create posts"),
            ("forum.posts.update", "Update posts"),
            ("forum.posts.delete", "Delete posts"),
            ("forum.comments.read", "Read comments"),
            ("forum.comments.create", "Create comments"),
            ("forum.comments.update", "Update comments"),
            ("forum.comments.delete", "Delete comments"),
            ("forum.categories.read", "Read categories"),
            ("forum.categories.create", "Create categories"),
            ("forum.categories.update", "Update categories"),
            ("forum.categories.delete", "Delete categories"),
            ("forum.tags.read", "Read tags"),
            ("forum.tags.create", "Create tags"),
            ("forum.tags.update", "Update tags"),
            ("forum.tags.delete", "Delete tags"),
            ("forum.reports.review", "Review reports"),
            ("forum.thread_channels.manage", "Manage forum thread channels"),
            ("learning.courses.read", "Read courses"),
            ("learning.courses.create", "Create courses"),
            ("learning.courses.update", "Update courses"),
            ("learning.courses.delete", "Delete courses"),
            ("learning.documents.read", "Read documents"),
            ("learning.documents.create", "Create documents"),
            ("learning.documents.update", "Update documents"),
            ("learning.documents.delete", "Delete documents"),
            ("learning.documents.approve", "Approve documents"),
            ("learning.documents.reject", "Reject documents"),
            ("learning.documents.request_revision", "Request document revision"),
            ("learning.faculties.read", "Read faculties"),
            ("learning.faculties.create", "Create faculties"),
            ("learning.faculties.update", "Update faculties"),
            ("learning.faculties.delete", "Delete faculties"),
            ("career.companies.read", "Read companies"),
            ("career.companies.create", "Create companies"),
            ("career.companies.update", "Update companies"),
            ("career.companies.delete", "Delete companies"),
            ("career.jobpostings.read", "Read job postings"),
            ("career.jobpostings.create", "Create job postings"),
            ("career.jobpostings.update", "Update job postings"),
            ("career.jobpostings.delete", "Delete job postings"),
            ("chat.channels.read", "Read channels"),
            ("chat.channels.create", "Create channels"),
            ("chat.channels.update", "Update channels"),
            ("chat.channels.delete", "Delete channels"),
            ("notification.notifications.read", "Read notifications"),
            ("notification.notifications.create", "Create notifications"),
            ("notification.notifications.update", "Update notifications"),
            ("notification.notifications.delete", "Delete notifications"),
            ("admin.system.manage", "Full system management access"),
        };

        return permissionData
            .Select(p => Permission.Create(p.Item1, p.Item2).Value)
            .ToList();
    }
}
