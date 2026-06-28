using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Application.Commands.CreateTag;
using UniHub.Forum.Domain.Tags;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Forum.Application.Commands.UpdateTag;

public sealed class UpdateTagCommandHandler : ICommandHandler<UpdateTagCommand>
{
    private readonly ITagRepository _tagRepository;

    public UpdateTagCommandHandler(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    public async Task<Result> Handle(UpdateTagCommand command, CancellationToken cancellationToken)
    {
        var tag = await _tagRepository.GetByIdAsync(new TagId(command.TagId), cancellationToken);
        if (tag == null)
        {
            return Result.Failure(TagErrors.TagNotFound);
        }

        // Check if another tag with the same name exists
        var existingTag = await _tagRepository.GetByNameAsync(command.Name, cancellationToken);
        if (existingTag != null && existingTag.Id.Value != command.TagId)
        {
            return Result.Failure(TagErrors.TagAlreadyExists);
        }

        var updateResult = tag.Update(command.Name, command.Description);
        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        await _tagRepository.UpdateAsync(tag, cancellationToken);

        return Result.Success();
    }
}
