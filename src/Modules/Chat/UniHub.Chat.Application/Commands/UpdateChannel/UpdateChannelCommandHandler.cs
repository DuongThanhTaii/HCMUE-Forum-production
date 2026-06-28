using UniHub.Chat.Application.Abstractions;
using UniHub.Chat.Domain.Channels;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Chat.Application.Commands.UpdateChannel;

/// <summary>
/// Handler for updating channel information
/// </summary>
public sealed class UpdateChannelCommandHandler : ICommandHandler<UpdateChannelCommand>
{
    private readonly IChannelRepository _channelRepository;

    public UpdateChannelCommandHandler(IChannelRepository channelRepository)
    {
        _channelRepository = channelRepository;
    }

    public async Task<Result> Handle(
        UpdateChannelCommand request,
        CancellationToken cancellationToken)
    {
        // Get channel
        var channelId = ChannelId.Create(request.ChannelId);
        var channel = await _channelRepository.GetByIdAsync(channelId, cancellationToken);

        if (channel is null)
        {
            return Result.Failure(new Error(
                "Channel.NotFound",
                $"Channel with ID {request.ChannelId} not found"));
        }

        // Update channel using domain method
        // Use current values if not provided in request
        var newName = request.Name ?? channel.Name;
        var newDescription = request.Description ?? channel.Description;
        
        var result = channel.UpdateInfo(
            newName,
            newDescription,
            request.UpdatedBy);

        if (result.IsFailure)
        {
            return result;
        }

        // Update channel
        await _channelRepository.UpdateAsync(channel, cancellationToken);

        return Result.Success();
    }
}
