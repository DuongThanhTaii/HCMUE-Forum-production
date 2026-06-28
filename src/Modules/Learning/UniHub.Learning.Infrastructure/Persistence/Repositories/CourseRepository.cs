using Microsoft.EntityFrameworkCore;
using UniHub.Learning.Application.Abstractions;
using UniHub.Learning.Domain.Courses;
using UniHub.Infrastructure.Persistence;

namespace UniHub.Learning.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of ICourseRepository.
/// </summary>
internal sealed class CourseRepository : ICourseRepository
{
    private readonly ApplicationDbContext _context;

    public CourseRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task AddAsync(Course course, CancellationToken cancellationToken = default)
    {
        _context.Courses.Add(course);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Course course, CancellationToken cancellationToken = default)
    {
        _context.Courses.Update(course);
        return Task.CompletedTask;
    }

    public async Task<Course?> GetByIdAsync(CourseId id, CancellationToken cancellationToken = default)
    {
        return await _context.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsAsync(CourseId id, CancellationToken cancellationToken = default)
    {
        return await _context.Courses
            .AsNoTracking()
            .AnyAsync(c => c.Id == id, cancellationToken);
    }

    public async Task DeleteAsync(CourseId id, CancellationToken cancellationToken = default)
    {
        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        
        if (course is not null)
        {
            _context.Courses.Remove(course);
        }
    }

    public async Task<IReadOnlyList<Course>> GetByFacultyIdAsync(Guid facultyId, CancellationToken cancellationToken = default)
    {
        return await _context.Courses
            .AsNoTracking()
            .Where(c => c.FacultyId == facultyId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Course>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Courses
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.Courses
            .AsNoTracking()
            .AnyAsync(c => c.Code == code, cancellationToken);
    }

    public async Task<IReadOnlyList<Course>> GetByModeratorIdAsync(Guid moderatorId, CancellationToken cancellationToken = default)
    {
        // TODO: This requires JSONB query on moderator_ids array
        // For now, load all courses and filter in-memory
        var allCourses = await _context.Courses
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return allCourses
            .Where(c => c.ModeratorIds.Contains(moderatorId))
            .ToList();
    }

    public async Task<(IReadOnlyList<Course> Items, int TotalCount)> SearchPagedAsync(
        Guid? facultyId,
        string? semester,
        string? searchTerm,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _context.Courses.AsNoTracking();

        if (facultyId.HasValue)
        {
            query = query.Where(c => c.FacultyId == facultyId.Value);
        }

        var semesterFilter = semester?.Trim();
        if (!string.IsNullOrEmpty(semesterFilter))
        {
            query = query.Where(c =>
                EF.Functions.ILike(c.Semester.Value, semesterFilter));
        }

        var search = searchTerm?.Trim();
        if (!string.IsNullOrEmpty(search))
        {
            var pattern = $"%{EscapeForLike(search)}%";
            query = query.Where(c =>
                EF.Functions.ILike(c.Name.Value, pattern) ||
                EF.Functions.ILike(c.Code.Value, pattern));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(c => c.Code.Value)
            .ThenBy(c => c.Name.Value)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<string>> GetDistinctSemestersAsync(
        Guid? facultyId,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Courses.AsNoTracking();

        if (facultyId.HasValue)
        {
            query = query.Where(c => c.FacultyId == facultyId.Value);
        }

        return await query
            .Select(c => c.Semester.Value)
            .Distinct()
            .OrderBy(s => s)
            .ToListAsync(cancellationToken);
    }

    private static string EscapeForLike(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);
    }
}
