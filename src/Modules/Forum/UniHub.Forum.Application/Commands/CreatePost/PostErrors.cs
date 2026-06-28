using UniHub.SharedKernel.Results;

namespace UniHub.Forum.Application.Commands.CreatePost;

/// <summary>
/// Common post-related errors
/// </summary>
public static class PostErrors
{
    public static readonly Error CategoryNotFound = new(
        "Post.CategoryNotFound",
        "The specified category does not exist");

    public static readonly Error InvalidPostType = new(
        "Post.InvalidPostType",
        "The specified post type is invalid");

    public static readonly Error ThreadChannelNotFound = new(
        "Post.ThreadChannelNotFound",
        "The specified thread channel does not exist or is inactive");

    public static readonly Error PostNotFound = new(
        "Post.NotFound",
        "The specified post does not exist");

    public static readonly Error UnauthorizedAccess = new(
        "Post.UnauthorizedAccess",
        "You are not authorized to perform this action");
}
