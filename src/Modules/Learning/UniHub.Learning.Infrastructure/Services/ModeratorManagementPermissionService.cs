using Microsoft.EntityFrameworkCore;
using UniHub.Learning.Application.Abstractions;
using UniHub.Learning.Domain.Courses;
using UniHub.Infrastructure.Persistence;

namespace UniHub.Learning.Infrastructure.Services;

/// <summary>
/// Service for checking if a user can manage moderators for a course.
/// Currently checks if user is already a moderator (can manage other moderators).
/// TODO: Integrate with Identity module to check for admin/manager roles.
/// </summary>
internal sealed class ModeratorManagementPermissionService : IModeratorManagementPermissionService
{
    private readonly ApplicationDbContext _context;

    public ModeratorManagementPermissionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CanManageCourseModeratorsAsync(
        Guid userId,
        Guid courseId,
        CancellationToken cancellationToken = default)
    {
        // TODO: This should integrate with Identity module to check for:
        // - Admin role
        // - Course manager role
        // - Faculty admin role
        
        // For now, check if user is already a moderator of the course
        // (moderators can manage other moderators)
        var course = await _context.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == CourseId.Create(courseId), cancellationToken);

        if (course is null)
        {
            return false;
        }

        // Allow moderators to manage other moderators
        return course.ModeratorIds.Contains(userId);
    }
}
