using UniHub.Forum.Domain.Posts;
using UniHub.Forum.Domain.Votes;
using UniHub.SharedKernel.Domain;

namespace UniHub.Forum.Domain.Events;

public sealed record PostVoteChangedEvent(PostId PostId, Guid UserId, VoteType OldVoteType, VoteType NewVoteType) : IDomainEvent;
