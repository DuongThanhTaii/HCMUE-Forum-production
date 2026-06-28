using Microsoft.EntityFrameworkCore;
using UniHub.Infrastructure.Persistence;

namespace UniHub.Forum.Infrastructure.Persistence;

/// <summary>
/// Resolves category and user display strings for forum lists (batch-friendly).
/// EF Core 10 cannot translate Contains/equality on value-object-converted PK columns.
/// All queries use raw SQL with PostgreSQL ANY(@array) to avoid this limitation.
/// </summary>
internal static class DisplayNameLookup
{
    public static async Task<Dictionary<Guid, string>> LoadCategoryNamesAsync(
        ApplicationDbContext context,
        IReadOnlyCollection<Guid> categoryIds,
        CancellationToken cancellationToken)
    {
        if (categoryIds.Count == 0)
        {
            return new Dictionary<Guid, string>();
        }

        var idArray = categoryIds.ToArray();
        var rows = await context.Database
            .SqlQuery<CategoryNameRow>($"""
                SELECT id AS "Id", name AS "Name"
                FROM forum.categories
                WHERE id = ANY({idArray})
                """)
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(x => x.Id, x => x.Name);
    }

    public static async Task<Dictionary<Guid, string>> LoadAuthorNamesAsync(
        ApplicationDbContext context,
        IReadOnlyCollection<Guid> authorIds,
        CancellationToken cancellationToken)
    {
        if (authorIds.Count == 0)
        {
            return new Dictionary<Guid, string>();
        }

        var idArray = authorIds.ToArray();
        var rows = await context.Database
            .SqlQuery<AuthorNameRow>($"""
                SELECT id AS "Id", first_name AS "FirstName", last_name AS "LastName", email AS "Email"
                FROM identity.users
                WHERE id = ANY({idArray})
                """)
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(r => r.Id, r => FormatPersonName(r.FirstName, r.LastName, r.Email));
    }

    private static string FormatPersonName(string firstName, string lastName, string email)
    {
        var full = $"{firstName} {lastName}".Trim();
        return string.IsNullOrWhiteSpace(full) ? email : full;
    }

    private sealed record CategoryNameRow(Guid Id, string Name);
    private sealed record AuthorNameRow(Guid Id, string FirstName, string LastName, string Email);
}
