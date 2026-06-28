using UniHub.Forum.Domain.Comments;
using UniHub.Forum.Domain.Votes;
using UniHub.SharedKernel.Domain;

namespace UniHub.Forum.Domain.Events;

public sealed record CommentVoteAddedEvent(CommentId CommentId, Guid UserId, VoteType VoteType) : IDomainEvent;
