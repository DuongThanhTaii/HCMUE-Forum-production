using UniHub.Forum.Domain.Posts;
using UniHub.SharedKernel.Domain;

namespace UniHub.Forum.Domain.Events;

public sealed record PostArchivedEvent(PostId PostId) : IDomainEvent;
