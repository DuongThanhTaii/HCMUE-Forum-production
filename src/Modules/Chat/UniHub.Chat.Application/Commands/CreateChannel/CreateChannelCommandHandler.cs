using UniHub.Chat.Application.Abstractions;
using UniHub.Chat.Domain.Channels;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Chat.Application.Commands.CreateChannel;

/// <summary>
/// Handler for creating a channel
/// </summary>
public sealed class CreateChannelCommandHandler : ICommandHandler<CreateChannelCommand, Guid>
{
    private readonly IChannelRepository _channelRepository;

    public CreateChannelCommandHandler(IChannelRepository channelRepository)
    {
        _channelRepository = channelRepository;
    }

    public async Task<Result<Guid>> Handle(
        CreateChannelCommand request,
        CancellationToken cancellationToken)
    {
        // Determine channel type
        var channelType = request.IsPublic ? ChannelType.Public : ChannelType.Private;

        // Create channel using domain factory
        var channelResult = Channel.Create(
            request.Name,
            channelType,
            request.OwnerId,
            request.Description);

        if (channelResult.IsFailure)
        {
            return Result.Failure<Guid>(channelResult.Error);
        }

        var channel = channelResult.Value;

        // Persist channel
        await _channelRepository.AddAsync(channel, cancellationToken);

        return Result.Success(channel.Id.Value);
    }
}
