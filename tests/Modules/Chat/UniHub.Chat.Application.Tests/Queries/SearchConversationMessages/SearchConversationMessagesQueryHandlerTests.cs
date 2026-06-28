using NSubstitute;
using UniHub.Chat.Application.Abstractions;
using UniHub.Chat.Application.Queries.SearchConversationMessages;
using UniHub.Chat.Domain.Conversations;
using UniHub.Chat.Domain.Messages;

namespace UniHub.Chat.Application.Tests.Queries.SearchConversationMessages;

public class SearchConversationMessagesQueryHandlerTests
{
    private readonly IConversationRepository _conversations = Substitute.For<IConversationRepository>();
    private readonly IMessageRepository _messages = Substitute.For<IMessageRepository>();
    private readonly IConversationParticipantLookup _participantLookup =
        Substitute.For<IConversationParticipantLookup>();

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _otherUserId = Guid.NewGuid();

    [Fact]
    public async Task Handle_WhenUserNotParticipant_ShouldReturnForbiddenError()
    {
        var conversation = Conversation.CreateDirect(_userId, _otherUserId, _userId).Value;
        var outsider = Guid.NewGuid();
        _conversations
            .GetByIdAsync(conversation.Id, Arg.Any<CancellationToken>())
            .Returns(conversation);

        var handler = new SearchConversationMessagesQueryHandler(
            _conversations,
            _messages,
            _participantLookup);

        var result = await handler.Handle(
            new SearchConversationMessagesQuery(
                outsider,
                conversation.Id.Value,
                "hello"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conversation.NotParticipant");
    }

    [Fact]
    public async Task Handle_WhenParticipant_ShouldReturnPagedHits()
    {
        var conversation = Conversation.CreateDirect(_userId, _otherUserId, _userId).Value;
        var message = Message.CreateText(
            conversation.Id,
            _otherUserId,
            "EF Core tips",
            null).Value;

        _conversations
            .GetByIdAsync(conversation.Id, Arg.Any<CancellationToken>())
            .Returns(conversation);
        _messages
            .SearchByConversationIdAsync(
                conversation.Id,
                "EF",
                ConversationMessageSearchFilter.All,
                1,
                20,
                Arg.Any<CancellationToken>())
            .Returns(([message], 1));
        _participantLookup
            .GetByIdsAsync(Arg.Any<IReadOnlyList<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, ParticipantDisplay>
            {
                [_otherUserId] = new ParticipantDisplay("Peer User", "peer@test.com"),
            });

        var handler = new SearchConversationMessagesQueryHandler(
            _conversations,
            _messages,
            _participantLookup);

        var result = await handler.Handle(
            new SearchConversationMessagesQuery(
                _userId,
                conversation.Id.Value,
                "EF"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items[0].MessageId.Should().Be(message.Id.Value);
        result.Value.Items[0].Snippet.Should().Contain("EF Core");
    }
}
