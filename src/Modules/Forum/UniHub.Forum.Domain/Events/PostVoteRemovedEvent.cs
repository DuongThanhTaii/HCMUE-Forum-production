using UniHub.Forum.Domain.Posts;
using UniHub.Forum.Domain.Votes;
using UniHub.SharedKernel.Domain;

namespace UniHub.Forum.Domain.Events;

public sealed record PostVoteRemovedEvent(PostId PostId, Guid UserId, VoteType VoteType) : IDomainEvent;
