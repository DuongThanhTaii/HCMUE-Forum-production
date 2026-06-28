using UniHub.Forum.Domain.Comments;
using UniHub.Forum.Domain.Votes;
using UniHub.SharedKernel.Domain;

namespace UniHub.Forum.Domain.Events;

public sealed record CommentVoteChangedEvent(CommentId CommentId, Guid UserId, VoteType OldVoteType, VoteType NewVoteType) : IDomainEvent;
