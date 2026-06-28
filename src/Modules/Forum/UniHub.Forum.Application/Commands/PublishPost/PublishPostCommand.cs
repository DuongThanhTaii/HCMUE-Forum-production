using UniHub.SharedKernel.CQRS;

namespace UniHub.Forum.Application.Commands.PublishPost;

public enum PostPublishActor
{
    Author = 0,
    Admin = 1,
    Moderator = 2
}

/// <summary>
/// Command to publish a post
/// </summary>
public sealed record PublishPostCommand(
    Guid PostId,
    Guid RequestingUserId,
    PostPublishActor Actor = PostPublishActor.Author) : ICommand;
