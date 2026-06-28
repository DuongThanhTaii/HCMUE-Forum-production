using UniHub.Forum.Domain.Comments;
using UniHub.Forum.Domain.Posts;
using UniHub.SharedKernel.Domain;

namespace UniHub.Forum.Domain.Events;

public sealed record CommentUnacceptedAsAnswerEvent(CommentId CommentId, PostId PostId) : IDomainEvent;
