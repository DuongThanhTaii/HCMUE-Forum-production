using UniHub.SharedKernel.Results;

namespace UniHub.Forum.Application.Commands.BookmarkPost;

public static class BookmarkErrors
{
    public static readonly Error PostNotFound = new(
        "Bookmark.PostNotFound",
        "The specified post does not exist");

    public static readonly Error AlreadyBookmarked = new(
        "Bookmark.AlreadyBookmarked",
        "This post is already bookmarked");

    public static readonly Error BookmarkNotFound = new(
        "Bookmark.NotFound",
        "Bookmark not found");
}
