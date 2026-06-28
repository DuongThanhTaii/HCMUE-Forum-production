using UniHub.SharedKernel.CQRS;

namespace UniHub.Forum.Application.Commands.DeletePost;

/// <summary>
/// Command to delete a post
/// </summary>
public sealed record DeletePostCommand(
    Guid PostId,
    Guid RequestingUserId) : ICommand;
