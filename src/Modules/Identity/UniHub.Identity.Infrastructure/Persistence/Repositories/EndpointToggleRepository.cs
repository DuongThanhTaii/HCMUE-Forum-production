using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Domain.Authorization;

namespace UniHub.Identity.Infrastructure.Persistence.Repositories;

public sealed class EndpointToggleRepository : IEndpointToggleRepository
{
    private static readonly List<EndpointToggle> Toggles = new();
    private static readonly object LockObj = new();
    private static bool _seeded;

    public EndpointToggleRepository()
    {
        EnsureSeeded();
    }

    public Task<EndpointToggle?> GetByEndpointKeyAsync(string endpointKey, CancellationToken cancellationToken = default)
    {
        lock (LockObj)
        {
            var result = Toggles.FirstOrDefault(item =>
                item.EndpointKey.Equals(endpointKey, StringComparison.OrdinalIgnoreCase));

            return Task.FromResult(result);
        }
    }

    public Task<List<EndpointToggle>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        lock (LockObj)
        {
            return Task.FromResult(Toggles.ToList());
        }
    }

    public Task AddAsync(EndpointToggle endpointToggle, CancellationToken cancellationToken = default)
    {
        lock (LockObj)
        {
            Toggles.Add(endpointToggle);
        }

        return Task.CompletedTask;
    }

    public Task UpdateAsync(EndpointToggle endpointToggle, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    private static void EnsureSeeded()
    {
        lock (LockObj)
        {
            if (_seeded || Toggles.Count > 0)
            {
                _seeded = true;
                return;
            }

            var nowUser = "system-seed";
            var seedKeys = new[]
            {
                "UniHub.Forum.Posts.Create",
                "UniHub.Forum.Posts.Update",
                "UniHub.Forum.Posts.Delete",
                "UniHub.Forum.Comments.Delete",
                "UniHub.Learning.Documents.Approve",
                "UniHub.Learning.Documents.Reject",
                "UniHub.Identity.Authorization.Toggles",
            };

            foreach (var key in seedKeys)
            {
                var created = EndpointToggle.Create(key, true, nowUser, "Initial seeded toggle");
                if (created.IsSuccess)
                {
                    Toggles.Add(created.Value);
                }
            }

            _seeded = true;
        }
    }
}
