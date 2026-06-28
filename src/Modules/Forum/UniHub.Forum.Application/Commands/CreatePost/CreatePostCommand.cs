using UniHub.SharedKernel.CQRS;

namespace UniHub.Forum.Application.Commands.CreatePost;

/// <summary>
/// Command to create a new post
/// </summary>
public sealed record CreatePostCommand(
    string Title,
    string Content,
    int Type,
    Guid AuthorId,
    Guid? CategoryId = null,
    Guid? ThreadChannelId = null,
    IEnumerable<string>? Tags = null) : ICommand<Guid>;
