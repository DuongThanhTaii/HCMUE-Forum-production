using UniHub.Forum.Domain.Posts;
using UniHub.SharedKernel.Domain;

namespace UniHub.Forum.Domain.Events;

public sealed record PostDeletedEvent(PostId PostId, Guid AuthorId) : IDomainEvent;
