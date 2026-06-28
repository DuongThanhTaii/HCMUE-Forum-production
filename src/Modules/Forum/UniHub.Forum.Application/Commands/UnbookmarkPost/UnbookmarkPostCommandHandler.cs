using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Application.Commands.BookmarkPost;
using UniHub.Forum.Domain.Posts;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Forum.Application.Commands.UnbookmarkPost;

public sealed class UnbookmarkPostCommandHandler : ICommandHandler<UnbookmarkPostCommand>
{
    private readonly IBookmarkRepository _bookmarkRepository;

    public UnbookmarkPostCommandHandler(IBookmarkRepository bookmarkRepository)
    {
        _bookmarkRepository = bookmarkRepository;
    }

    public async Task<Result> Handle(UnbookmarkPostCommand command, CancellationToken cancellationToken)
    {
        var postId = new PostId(command.PostId);
        var bookmark = await _bookmarkRepository.GetByUserAndPostAsync(
            command.UserId,
            postId,
            cancellationToken);

        if (bookmark == null)
        {
            return Result.Failure(BookmarkErrors.BookmarkNotFound);
        }

        await _bookmarkRepository.RemoveAsync(bookmark, cancellationToken);

        return Result.Success();
    }
}
