using UniHub.Forum.Domain.Posts;
using UniHub.SharedKernel.Domain;

namespace UniHub.Forum.Domain.Events;

public sealed record PostPinnedEvent(PostId PostId) : IDomainEvent;
