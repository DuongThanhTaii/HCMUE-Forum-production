using Microsoft.EntityFrameworkCore;
using UniHub.Identity.Domain.Users;
using UniHub.Learning.Application.Abstractions;
using UniHub.Learning.Domain.Courses;
using UniHub.Infrastructure.Persistence;

namespace UniHub.Learning.Infrastructure.Services;

internal sealed class DocumentDetailEnricher : IDocumentDetailEnricher
{
    private readonly ApplicationDbContext _context;

    public DocumentDetailEnricher(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(string? UploaderDisplayName, string? CourseName, string? ReviewerDisplayName)> EnrichAsync(
        Guid uploaderId,
        Guid? courseId,
        Guid? reviewerId,
        CancellationToken cancellationToken = default)
    {
        var uploaderName = await ResolveUserDisplayNameAsync(uploaderId, cancellationToken);

        string? courseName = null;
        if (courseId.HasValue)
        {
            // Create value object OUTSIDE the expression tree so EF can parameterize it correctly.
            var courseIdVo = CourseId.Create(courseId.Value);
            courseName = await _context.Courses
                .AsNoTracking()
                .Where(c => c.Id == courseIdVo)
                .Select(c => c.Name.Value)
                .FirstOrDefaultAsync(cancellationToken);
        }

        string? reviewerName = null;
        if (reviewerId.HasValue)
        {
            reviewerName = await ResolveUserDisplayNameAsync(reviewerId.Value, cancellationToken);
        }

        return (uploaderName, string.IsNullOrWhiteSpace(courseName) ? null : courseName, reviewerName);
    }

    private async Task<string?> ResolveUserDisplayNameAsync(Guid userId, CancellationToken cancellationToken)
    {
        var userIdVo = UserId.Create(userId);
        var row = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == userIdVo)
            .Select(u => new
            {
                u.Profile.FirstName,
                u.Profile.LastName,
                Email = u.Email.Value,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            return null;
        }

        var full = $"{row.FirstName} {row.LastName}".Trim();
        return string.IsNullOrWhiteSpace(full) ? row.Email : full;
    }
}
