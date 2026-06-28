using UniHub.Chat.Application.Queries.SearchConversationMessages;

namespace UniHub.Chat.Application.Tests.Queries.SearchConversationMessages;

public class SearchConversationMessagesQueryValidatorTests
{
    private readonly SearchConversationMessagesQueryValidator _validator = new();

    [Fact]
    public void Validate_WithValidQuery_ShouldPass()
    {
        var query = new SearchConversationMessagesQuery(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "EF Core",
            "text",
            1,
            20);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithShortQuery_ShouldFail()
    {
        var query = new SearchConversationMessagesQuery(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "a");

        var result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
    }
}
