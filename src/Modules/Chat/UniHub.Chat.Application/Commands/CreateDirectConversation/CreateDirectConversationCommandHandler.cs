using UniHub.Chat.Application.Abstractions;
using UniHub.Chat.Domain.Conversations;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Chat.Application.Commands.CreateDirectConversation;

/// <summary>
/// Handler for creating a new direct conversation
/// </summary>
public sealed class CreateDirectConversationCommandHandler 
    : ICommandHandler<CreateDirectConversationCommand, Guid>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IUserBlockChecker _userBlockChecker;

    public CreateDirectConversationCommandHandler(
        IConversationRepository conversationRepository,
        IUserBlockChecker userBlockChecker)
    {
        _conversationRepository = conversationRepository;
        _userBlockChecker = userBlockChecker;
    }

    public async Task<Result<Guid>> Handle(
        CreateDirectConversationCommand request,
        CancellationToken cancellationToken)
    {
        // Check if direct conversation already exists
        var existingConversation = await _conversationRepository
            .GetDirectConversationAsync(request.User1Id, request.User2Id, cancellationToken);

        if (existingConversation is not null)
        {
            // Return existing conversation ID instead of creating duplicate
            return Result.Success(existingConversation.Id.Value);
        }

        if (await _userBlockChecker.IsBlockedEitherWayAsync(
                request.User1Id,
                request.User2Id,
                cancellationToken))
        {
            return Result.Failure<Guid>(new Error(
                "Chat.UserBlocked",
                "You cannot message this user because one of you has blocked the other"));
        }

        // Create new direct conversation
        var conversationResult = Conversation.CreateDirect(
            request.User1Id,
            request.User2Id,
            request.CreatorId);

        if (conversationResult.IsFailure)
        {
            return Result.Failure<Guid>(conversationResult.Error);
        }

        var conversation = conversationResult.Value;

        // Save conversation
        await _conversationRepository.AddAsync(conversation, cancellationToken);

        return Result.Success(conversation.Id.Value);
    }
}
