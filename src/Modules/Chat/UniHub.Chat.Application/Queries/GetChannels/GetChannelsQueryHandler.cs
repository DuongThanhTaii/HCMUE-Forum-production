using UniHub.Chat.Application.Abstractions;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Chat.Application.Queries.GetChannels;

/// <summary>
/// Handler for getting channels
/// </summary>
public sealed class GetChannelsQueryHandler : IQueryHandler<GetChannelsQuery, IReadOnlyList<ChannelResponse>>
{
    private readonly IChannelRepository _channelRepository;

    public GetChannelsQueryHandler(IChannelRepository channelRepository)
    {
        _channelRepository = channelRepository;
    }

    public async Task<Result<IReadOnlyList<ChannelResponse>>> Handle(
        GetChannelsQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<Domain.Channels.Channel> channels;

        // If UserId is provided, get user's channels
        if (request.UserId.HasValue)
        {
            channels = await _channelRepository.GetByMemberIdAsync(request.UserId.Value, cancellationToken);
        }
        // If PublicOnly is true, get all public channels
        else if (request.PublicOnly == true)
        {
            channels = await _channelRepository.GetPublicChannelsAsync(cancellationToken);
        }
        // Otherwise, get all public channels (default behavior for discovery)
        else
        {
            channels = await _channelRepository.GetPublicChannelsAsync(cancellationToken);
        }

        var response = channels
            .Where(c => !c.IsArchived)
            .Select(c => new ChannelResponse(
                c.Id.Value,
                c.Name,
                c.Description,
                c.Type.ToString(),
                c.OwnerId,
                c.Members.Count,
                c.CreatedAt,
                c.IsArchived))
            .ToList();

        return Result.Success<IReadOnlyList<ChannelResponse>>(response);
    }
}
