using UniHub.Chat.Application.Abstractions;
using UniHub.Chat.Domain.Channels;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Chat.Application.Commands.JoinChannel;

/// <summary>
/// Handler for joining a channel
/// </summary>
public sealed class JoinChannelCommandHandler : ICommandHandler<JoinChannelCommand>
{
    private readonly IChannelRepository _channelRepository;

    public JoinChannelCommandHandler(IChannelRepository channelRepository)
    {
        _channelRepository = channelRepository;
    }

    public async Task<Result> Handle(
        JoinChannelCommand request,
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

        // Join channel using domain method
        var result = channel.Join(request.UserId);

        if (result.IsFailure)
        {
            return result;
        }

        // Update channel
        await _channelRepository.UpdateAsync(channel, cancellationToken);

        return Result.Success();
    }
}
