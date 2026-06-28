using UniHub.SharedKernel.CQRS;

namespace UniHub.Forum.Application.Commands.PinPost;

/// <summary>
/// Command to pin a post
/// </summary>
public sealed record PinPostCommand(
    Guid PostId,
    Guid RequestingUserId) : ICommand;
