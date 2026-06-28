using UniHub.SharedKernel.CQRS;

namespace UniHub.Forum.Application.Commands.UpdatePost;

/// <summary>
/// Command to update an existing post
/// </summary>
public sealed record UpdatePostCommand(
    Guid PostId,
    string Title,
    string Content,
    Guid? CategoryId,
    Guid? ThreadChannelId,
    IEnumerable<string>? Tags,
    Guid RequestingUserId) : ICommand;
