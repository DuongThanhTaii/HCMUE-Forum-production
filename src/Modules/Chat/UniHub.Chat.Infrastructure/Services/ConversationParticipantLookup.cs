using Microsoft.EntityFrameworkCore;
using UniHub.Chat.Application.Abstractions;
using UniHub.Identity.Domain.Users;
using UniHub.Infrastructure.Persistence;

namespace UniHub.Chat.Infrastructure.Services;

/// <summary>
/// Loads display names for conversation participants from the identity store.
/// </summary>
public sealed class ConversationParticipantLookup : IConversationParticipantLookup
{
    private readonly ApplicationDbContext _context;

    public ConversationParticipantLookup(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyDictionary<Guid, ParticipantDisplay>> GetByIdsAsync(
        IReadOnlyList<Guid> userIds,
        CancellationToken cancellationToken = default)
    {
        if (userIds.Count == 0)
        {
            return new Dictionary<Guid, ParticipantDisplay>();
        }

        var distinctIds = userIds.Distinct().Select(UserId.Create).ToList();

        // Contains(Guid[]) against PK mapped as UserId breaks Npgsql array/converter typing.
        // Match on UserId so translation uses uuid IN (...) without mixed element types.
        var rows = await _context.Set<User>()
            .AsNoTracking()
            .Where(u => distinctIds.Contains(u.Id))
            .Select(u => new
            {
                Id = u.Id.Value,
                First = u.Profile.FirstName,
                Last = u.Profile.LastName,
                Email = u.Email.Value,
            })
            .ToListAsync(cancellationToken);

        var map = new Dictionary<Guid, ParticipantDisplay>();
        foreach (var r in rows)
        {
            var name = $"{r.First} {r.Last}".Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                name = r.Email;
            }

            map[r.Id] = new ParticipantDisplay(name, r.Email);
        }

        return map;
    }
}
