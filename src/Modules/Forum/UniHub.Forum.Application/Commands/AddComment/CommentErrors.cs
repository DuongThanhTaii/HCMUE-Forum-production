using UniHub.SharedKernel.Results;

namespace UniHub.Forum.Application.Commands.AddComment;

/// <summary>
/// Error definitions for comment operations
/// </summary>
public static class CommentErrors
{
    public static readonly Error PostNotFound = new(
        "Comment.PostNotFound",
        "The post for this comment does not exist");

    public static readonly Error ParentCommentNotFound = new(
        "Comment.ParentCommentNotFound",
        "The parent comment does not exist");

    public static readonly Error CommentNotFound = new(
        "Comment.NotFound",
        "The comment does not exist");

    public static readonly Error UnauthorizedAccess = new(
        "Comment.UnauthorizedAccess",
        "You are not authorized to perform this action on this comment");

    public static readonly Error InvalidContent = new(
        "Comment.InvalidContent",
        "The comment content is invalid");
}
