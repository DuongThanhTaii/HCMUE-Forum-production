using Microsoft.EntityFrameworkCore;
using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Domain.Categories;
using UniHub.Forum.Domain.Comments;
using UniHub.Forum.Domain.Posts;
using UniHub.Forum.Domain.Reports;
using UniHub.Infrastructure.Persistence;

namespace UniHub.Forum.Infrastructure.Services;

/// <summary>
/// Implements moderation category scope resolution for report listing and resolve authorization.
/// </summary>
internal sealed class ModerationScopeService : IModerationScopeService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ApplicationDbContext _context;

    public ModerationScopeService(
        ICategoryRepository categoryRepository,
        ApplicationDbContext context)
    {
        _categoryRepository = categoryRepository;
        _context = context;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Guid>?> GetCategoryScopeAsync(
        Guid userId,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        if (isAdmin)
        {
            // null = no filter; Admin sees all reports.
            return null;
        }

        var allCategories = await _categoryRepository.GetAllAsync(cancellationToken);
        var scope = allCategories
            .Where(c => c.ModeratorIds.Contains(userId))
            .Select(c => c.Id.Value)
            .ToList();

        return scope.AsReadOnly();
    }

    /// <inheritdoc />
    public async Task<Guid?> GetEffectiveCategoryIdAsync(
        Report report,
        CancellationToken cancellationToken = default)
    {
        if (report.ReportedItemType == ReportedItemType.Post)
        {
            var post = await _context.Posts
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == new PostId(report.ReportedItemId), cancellationToken);

            return post?.CategoryId;
        }

        // Comment: follow PostId → Post.CategoryId
        var comment = await _context.Comments
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == new CommentId(report.ReportedItemId), cancellationToken);

        if (comment is null)
        {
            return null;
        }

        var parentPost = await _context.Posts
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == comment.PostId, cancellationToken);

        return parentPost?.CategoryId;
    }
}
