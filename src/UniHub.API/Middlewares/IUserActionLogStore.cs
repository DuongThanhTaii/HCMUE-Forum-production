namespace UniHub.API.Middlewares;

using UniHub.API.Observability;

public interface IUserActionLogStore
{
    Task AppendAsync(UserActionLogEntry entry, CancellationToken cancellationToken = default);

    Task<UserActionLogSearchResult> SearchAsync(UserActionLogQuery query, CancellationToken cancellationToken = default);
}
