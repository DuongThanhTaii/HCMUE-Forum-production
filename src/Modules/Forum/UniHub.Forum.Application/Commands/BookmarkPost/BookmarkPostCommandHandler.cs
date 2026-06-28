using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Domain.Bookmarks;
using UniHub.Forum.Domain.Posts;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Forum.Application.Commands.BookmarkPost;

public sealed class BookmarkPostCommandHandler : ICommandHandler<BookmarkPostCommand>
{
    private readonly IPostRepository _postRepository;
    private readonly IBookmarkRepository _bookmarkRepository;

    public BookmarkPostCommandHandler(
        IPostRepository postRepository,
        IBookmarkRepository bookmarkRepository)
    {
        _postRepository = postRepository;
        _bookmarkRepository = bookmarkRepository;
    }

    public async Task<Result> Handle(BookmarkPostCommand command, CancellationToken cancellationToken)
    {
        // Check if post exists
        var postId = new PostId(command.PostId);
        var post = await _postRepository.GetByIdAsync(postId, cancellationToken);
        if (post == null)
        {
            return Result.Failure(BookmarkErrors.PostNotFound);
        }

        // Check if already bookmarked
        var existingBookmark = await _bookmarkRepository.GetByUserAndPostAsync(
            command.UserId,
            postId,
            cancellationToken);

        if (existingBookmark != null)
        {
            return Result.Failure(BookmarkErrors.AlreadyBookmarked);
        }

        // Create bookmark
        var bookmark = Bookmark.Create(postId, command.UserId);
        await _bookmarkRepository.AddAsync(bookmark, cancellationToken);

        return Result.Success();
    }
}
