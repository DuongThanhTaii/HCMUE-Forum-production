using UniHub.Chat.Application.Abstractions;
using UniHub.Chat.Domain.Channels;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Chat.Application.Commands.AddModerator;

/// <summary>
/// Handler for adding a moderator to a channel
/// </summary>
public sealed class AddModeratorCommandHandler : ICommandHandler<AddModeratorCommand>
{
    private readonly IChannelRepository _channelRepository;

    public AddModeratorCommandHandler(IChannelRepository channelRepository)
    {
        _channelRepository = channelRepository;
    }

    public async Task<Result> Handle(
        AddModeratorCommand request,
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

        // Add moderator using domain method
        var result = channel.AddModerator(request.UserId, request.AddedBy);

        if (result.IsFailure)
        {
            return result;
        }

        // Update channel
        await _channelRepository.UpdateAsync(channel, cancellationToken);

        return Result.Success();
    }
}
