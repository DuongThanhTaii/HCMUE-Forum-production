using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Application.Commands.CreateTag;
using UniHub.Forum.Domain.Tags;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Forum.Application.Commands.DeleteTag;

public sealed class DeleteTagCommandHandler : ICommandHandler<DeleteTagCommand>
{
    private readonly ITagRepository _tagRepository;

    public DeleteTagCommandHandler(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    public async Task<Result> Handle(DeleteTagCommand command, CancellationToken cancellationToken)
    {
        var tag = await _tagRepository.GetByIdAsync(new TagId(command.TagId), cancellationToken);
        if (tag == null)
        {
            return Result.Failure(TagErrors.TagNotFound);
        }

        // Check if tag is in use
        if (tag.UsageCount > 0)
        {
            return Result.Failure(new Error(
                TagErrors.TagInUse.Code,
                $"{TagErrors.TagInUse.Message} ({tag.UsageCount} posts)"));
        }

        await _tagRepository.DeleteAsync(tag, cancellationToken);

        return Result.Success();
    }
}
