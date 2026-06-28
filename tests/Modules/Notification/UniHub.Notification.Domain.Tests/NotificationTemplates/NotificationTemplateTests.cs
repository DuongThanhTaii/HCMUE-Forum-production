using UniHub.Notification.Domain.NotificationTemplates;
using UniHub.Notification.Domain.NotificationTemplates.Events;

namespace UniHub.Notification.Domain.Tests.NotificationTemplates;

public class NotificationTemplateTests
{
    private static readonly Guid TestUserId = Guid.NewGuid();

    #region Helper Methods

    private static NotificationTemplate CreateValidTemplate()
    {
        var channels = new List<NotificationChannel> { NotificationChannel.Email };
        return NotificationTemplate.Create(
            "WelcomeEmail",
            "Welcome Email Template",
            "Sent to new users upon registration",
            NotificationCategory.Account,
            channels,
            TestUserId).Value;
    }

    private static NotificationTemplate CreateTemplateWithAllChannels()
    {
        var channels = new List<NotificationChannel>
        {
            NotificationChannel.Email,
            NotificationChannel.Push,
            NotificationChannel.InApp
        };
        return NotificationTemplate.Create(
            "JobPosted",
            "Job Posted Notification",
            "Notify users when a new job is posted",
            NotificationCategory.Career,
            channels,
            TestUserId).Value;
    }

    #endregion

    #region Create Tests

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        var channels = new List<NotificationChannel> { NotificationChannel.Email };
        var result = NotificationTemplate.Create(
            "WelcomeEmail",
            "Welcome Email Template",
            "Sent to new users",
            NotificationCategory.Account,
            channels,
            TestUserId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("WelcomeEmail");
        result.Value.DisplayName.Should().Be("Welcome Email Template");
        result.Value.Description.Should().Be("Sent to new users");
        result.Value.Category.Should().Be(NotificationCategory.Account);
        result.Value.Status.Should().Be(NotificationTemplateStatus.Draft);
        result.Value.CreatedBy.Should().Be(TestUserId);
        result.Value.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.Value.Channels.Should().BeEquivalentTo(channels);
    }

    [Fact]
    public void Create_ShouldRaiseDomainEvent()
    {
        var channels = new List<NotificationChannel> { NotificationChannel.Email };
        var result = NotificationTemplate.Create(
            "Test",
            "Test Template",
            null,
            NotificationCategory.System,
            channels,
            TestUserId);

        result.Value.DomainEvents.Should().ContainSingle();
        var domainEvent = result.Value.DomainEvents.First();
        domainEvent.Should().BeOfType<NotificationTemplateCreatedEvent>();

        var createdEvent = (NotificationTemplateCreatedEvent)domainEvent;
        createdEvent.Name.Should().Be("Test");
        createdEvent.DisplayName.Should().Be("Test Template");
        createdEvent.Category.Should().Be(NotificationCategory.System);
        createdEvent.CreatedBy.Should().Be(TestUserId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Create_WithEmptyName_ShouldReturnFailure(string? name)
    {
        var channels = new List<NotificationChannel> { NotificationChannel.Email };
        var result = NotificationTemplate.Create(
            name!,
            "Display Name",
            null,
            NotificationCategory.Account,
            channels,
            TestUserId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(NotificationTemplateErrors.NameEmpty);
    }

    [Fact]
    public void Create_WithNameTooLong_ShouldReturnFailure()
    {
        var longName = new string('N', NotificationTemplate.MaxNameLength + 1);
        var channels = new List<NotificationChannel> { NotificationChannel.Email };
        var result = NotificationTemplate.Create(
            longName,
            "Display Name",
            null,
            NotificationCategory.Account,
            channels,
            TestUserId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(NotificationTemplateErrors.NameTooLong);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Create_WithEmptyDisplayName_ShouldReturnFailure(string? displayName)
    {
        var channels = new List<NotificationChannel> { NotificationChannel.Email };
        var result = NotificationTemplate.Create(
            "Name",
            displayName!,
            null,
            NotificationCategory.Account,
            channels,
            TestUserId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(NotificationTemplateErrors.DisplayNameEmpty);
    }

    [Fact]
    public void Create_WithDisplayNameTooLong_ShouldReturnFailure()
    {
        var longDisplayName = new string('D', NotificationTemplate.MaxDisplayNameLength + 1);
        var channels = new List<NotificationChannel> { NotificationChannel.Email };
        var result = NotificationTemplate.Create(
            "Name",
            longDisplayName,
            null,
            NotificationCategory.Account,
            channels,
            TestUserId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(NotificationTemplateErrors.DisplayNameTooLong);
    }

    [Fact]
    public void Create_WithDescriptionTooLong_ShouldReturnFailure()
    {
        var longDescription = new string('D', NotificationTemplate.MaxDescriptionLength + 1);
        var channels = new List<NotificationChannel> { NotificationChannel.Email };
        var result = NotificationTemplate.Create(
            "Name",
            "Display Name",
            longDescription,
            NotificationCategory.Account,
            channels,
            TestUserId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(NotificationTemplateErrors.DescriptionTooLong);
    }

    [Fact]
    public void Create_WithNoChannels_ShouldReturnFailure()
    {
        var result = NotificationTemplate.Create(
            "Name",
            "Display Name",
            null,
            NotificationCategory.Account,
            new List<NotificationChannel>(),
            TestUserId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(NotificationTemplateErrors.NoChannels);
    }

    [Fact]
    public void Create_WithNullChannels_ShouldReturnFailure()
    {
        var result = NotificationTemplate.Create(
            "Name",
            "Display Name",
            null,
            NotificationCategory.Account,
            null!,
            TestUserId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(NotificationTemplateErrors.NoChannels);
    }

    #endregion

    #region Activate Tests

    [Fact]
    public void Activate_WithCompleteContent_ShouldSucceed()
    {
        var template = CreateValidTemplate();
        var emailContent = EmailTemplateContent.Create(
            "Welcome",
            "Hello {UserName}",
            "UniHub",
            "noreply@unihub.com").Value;
        template.UpdateContent(emailContent, null, null, TestUserId);

        var result = template.Activate(TestUserId);

        result.IsSuccess.Should().BeTrue();
        template.Status.Should().Be(NotificationTemplateStatus.Active);
        template.ActivatedBy.Should().Be(TestUserId);
        template.ActivatedAt.Should().NotBeNull();
        template.ActivatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Activate_ShouldRaiseDomainEvent()
    {
        var template = CreateValidTemplate();
        var emailContent = EmailTemplateContent.Create("Subject", "Body").Value;
        template.UpdateContent(emailContent, null, null, TestUserId);
        template.ClearDomainEvents();

        template.Activate(TestUserId);

        template.DomainEvents.Should().ContainSingle();
        var domainEvent = template.DomainEvents.First();
        domainEvent.Should().BeOfType<NotificationTemplateActivatedEvent>();

        var activatedEvent = (NotificationTemplateActivatedEvent)domainEvent;
        activatedEvent.TemplateId.Should().Be(template.Id.Value);
        activatedEvent.Name.Should().Be(template.Name);
        activatedEvent.ActivatedBy.Should().Be(TestUserId);
    }

    [Fact]
    public void Activate_WhenEmailChannelEnabledButNoContent_ShouldReturnFailure()
    {
        var template = CreateValidTemplate();

        var result = template.Activate(TestUserId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(NotificationTemplateErrors.EmailChannelWithoutContent);
    }

    [Fact]
    public void Activate_WhenPushChannelEnabledButNoContent_ShouldReturnFailure()
    {
        var channels = new List<NotificationChannel> { NotificationChannel.Push };
        var template = NotificationTemplate.Create(
            "TestPush",
            "Test Push",
            null,
            NotificationCategory.Social,
            channels,
            TestUserId).Value;

        var result = template.Activate(TestUserId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(NotificationTemplateErrors.PushChannelWithoutContent);
    }

    [Fact]
    public void Activate_WhenInAppChannelEnabledButNoContent_ShouldReturnFailure()
    {
        var channels = new List<NotificationChannel> { NotificationChannel.InApp };
        var template = NotificationTemplate.Create(
            "TestInApp",
            "Test InApp",
            null,
            NotificationCategory.Social,
            channels,
            TestUserId).Value;

        var result = template.Activate(TestUserId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(NotificationTemplateErrors.InAppChannelWithoutContent);
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldReturnFailure()
    {
        var template = CreateValidTemplate();
        var emailContent = EmailTemplateContent.Create("Subject", "Body").Value;
        template.UpdateContent(emailContent, null, null, TestUserId);
        template.Activate(TestUserId);

        var result = template.Activate(TestUserId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(NotificationTemplateErrors.AlreadyActive);
    }

    [Fact]
    public void Activate_WhenArchived_ShouldReturnFailure()
    {
        var template = CreateValidTemplate();
        var emailContent = EmailTemplateContent.Create("Subject", "Body").Value;
        template.UpdateContent(emailContent, null, null, TestUserId);
        template.Activate(TestUserId);
        template.Archive(TestUserId);

        var result = template.Activate(TestUserId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(NotificationTemplateErrors.AlreadyArchived);
    }

    #endregion

    #region Archive Tests

    [Fact]
    public void Archive_WhenDraft_ShouldSucceed()
    {
        var template = CreateValidTemplate();

        var result = template.Archive(TestUserId);

        result.IsSuccess.Should().BeTrue();
        template.Status.Should().Be(NotificationTemplateStatus.Archived);
        template.UpdatedBy.Should().Be(TestUserId);
        template.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Archive_WhenActive_ShouldSucceed()
    {
        var template = CreateValidTemplate();
        var emailContent = EmailTemplateContent.Create("Subject", "Body").Value;
        template.UpdateContent(emailContent, null, null, TestUserId);
        template.Activate(TestUserId);

        var result = template.Archive(TestUserId);

        result.IsSuccess.Should().BeTrue();
        template.Status.Should().Be(NotificationTemplateStatus.Archived);
    }

    [Fact]
    public void Archive_ShouldRaiseDomainEvent()
    {
        var template = CreateValidTemplate();
        template.ClearDomainEvents();

        template.Archive(TestUserId);

        template.DomainEvents.Should().ContainSingle();
        var domainEvent = template.DomainEvents.First();
        domainEvent.Should().BeOfType<NotificationTemplateArchivedEvent>();

        var archivedEvent = (NotificationTemplateArchivedEvent)domainEvent;
        archivedEvent.TemplateId.Should().Be(template.Id.Value);
        archivedEvent.Name.Should().Be(template.Name);
        archivedEvent.ArchivedBy.Should().Be(TestUserId);
    }

    [Fact]
    public void Archive_WhenAlreadyArchived_ShouldReturnFailure()
    {
        var template = CreateValidTemplate();
        template.Archive(TestUserId);

        var result = template.Archive(TestUserId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(NotificationTemplateErrors.AlreadyArchived);
    }

    #endregion

    #region UpdateContent Tests

    [Fact]
    public void UpdateContent_WithEmailContent_ShouldSucceed()
    {
        var template = CreateValidTemplate();
        var emailContent = EmailTemplateContent.Create(
            "Welcome",
            "Hello {UserName}").Value;

        var result = template.UpdateContent(emailContent, null, null, TestUserId);

        result.IsSuccess.Should().BeTrue();
        template.EmailContent.Should().Be(emailContent);
        template.UpdatedBy.Should().Be(TestUserId);
        template.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateContent_WithAllChannelContent_ShouldSucceed()
    {
        var template = CreateTemplateWithAllChannels();
        var emailContent = EmailTemplateContent.Create("Subject", "Body").Value;
        var pushContent = PushTemplateContent.Create("Title", "Body").Value;
        var inAppContent = InAppTemplateContent.Create("Title", "Body").Value;

        var result = template.UpdateContent(emailContent, pushContent, inAppContent, TestUserId);

        result.IsSuccess.Should().BeTrue();
        template.EmailContent.Should().Be(emailContent);
        template.PushContent.Should().Be(pushContent);
        template.InAppContent.Should().Be(inAppContent);
    }

    [Fact]
    public void UpdateContent_ShouldRaiseDomainEvent()
    {
        var template = CreateValidTemplate();
        var emailContent = EmailTemplateContent.Create("Subject", "Body").Value;
        template.ClearDomainEvents();

        template.UpdateContent(emailContent, null, null, TestUserId);

        template.DomainEvents.Should().ContainSingle();
        var domainEvent = template.DomainEvents.First();
        domainEvent.Should().BeOfType<NotificationTemplateUpdatedEvent>();

        var updatedEvent = (NotificationTemplateUpdatedEvent)domainEvent;
        updatedEvent.TemplateId.Should().Be(template.Id.Value);
        updatedEvent.Name.Should().Be(template.Name);
        updatedEvent.UpdatedBy.Should().Be(TestUserId);
    }

    [Fact]
    public void UpdateContent_WithContentForDisabledChannel_ShouldReturnFailure()
    {
        var template = CreateValidTemplate(); // Only Email channel
        var pushContent = PushTemplateContent.Create("Title", "Body").Value;

        var result = template.UpdateContent(null, pushContent, null, TestUserId);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NotificationTemplate.PushChannelNotEnabled");
    }

    #endregion

    #region Variable Management Tests

    [Fact]
    public void AddVariable_WithValidVariable_ShouldSucceed()
    {
        var template = CreateValidTemplate();
        var variable = TemplateVariable.Create("UserName", "The user's name").Value;

        var result = template.AddVariable(variable);

        result.IsSuccess.Should().BeTrue();
        template.Variables.Should().ContainSingle();
        template.Variables.First().Should().Be(variable);
    }

    [Fact]
    public void AddVariable_MultipleVariables_ShouldSucceed()
    {
        var template = CreateValidTemplate();
        var var1 = TemplateVariable.Create("UserName", "Name").Value;
        var var2 = TemplateVariable.Create("Email", "Email address").Value;

        template.AddVariable(var1);
        template.AddVariable(var2);

        template.Variables.Should().HaveCount(2);
        template.Variables.Should().Contain(var1);
        template.Variables.Should().Contain(var2);
    }

    [Fact]
    public void AddVariable_WhenDuplicateName_ShouldReturnFailure()
    {
        var template = CreateValidTemplate();
        var var1 = TemplateVariable.Create("UserName", "Name 1").Value;
        var var2 = TemplateVariable.Create("UserName", "Name 2").Value;
        template.AddVariable(var1);

        var result = template.AddVariable(var2);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(NotificationTemplateErrors.DuplicateVariable);
        template.Variables.Should().ContainSingle();
    }

    [Fact]
    public void AddVariable_WhenDuplicateNameCaseInsensitive_ShouldReturnFailure()
    {
        var template = CreateValidTemplate();
        var var1 = TemplateVariable.Create("UserName", "Name 1").Value;
        var var2 = TemplateVariable.Create("USERNAME", "Name 2").Value;
        template.AddVariable(var1);

        var result = template.AddVariable(var2);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(NotificationTemplateErrors.DuplicateVariable);
    }

    [Fact]
    public void AddVariable_WhenMaxVariablesReached_ShouldReturnFailure()
    {
        var template = CreateValidTemplate();

        // Add MaxVariables number of variables
        for (int i = 0; i < NotificationTemplate.MaxVariables; i++)
        {
            var variable = TemplateVariable.Create($"Var{i}", "Description").Value;
            template.AddVariable(variable);
        }

        // Try to add one more
        var extraVariable = TemplateVariable.Create("ExtraVar", "Description").Value;
        var result = template.AddVariable(extraVariable);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(NotificationTemplateErrors.TooManyVariables);
    }

    [Fact]
    public void RemoveVariable_WithExistingVariable_ShouldSucceed()
    {
        var template = CreateValidTemplate();
        var variable = TemplateVariable.Create("UserName", "Name").Value;
        template.AddVariable(variable);

        var result = template.RemoveVariable("UserName");

        result.IsSuccess.Should().BeTrue();
        template.Variables.Should().BeEmpty();
    }

    [Fact]
    public void RemoveVariable_CaseInsensitive_ShouldSucceed()
    {
        var template = CreateValidTemplate();
        var variable = TemplateVariable.Create("UserName", "Name").Value;
        template.AddVariable(variable);

        var result = template.RemoveVariable("USERNAME");

        result.IsSuccess.Should().BeTrue();
        template.Variables.Should().BeEmpty();
    }

    [Fact]
    public void RemoveVariable_WhenNotFound_ShouldReturnFailure()
    {
        var template = CreateValidTemplate();

        var result = template.RemoveVariable("NonExistent");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(NotificationTemplateErrors.VariableNotFound);
    }

    #endregion

    #region Helper Method Tests

    [Fact]
    public void IsActive_WhenDraft_ShouldReturnFalse()
    {
        var template = CreateValidTemplate();

        template.IsActive().Should().BeFalse();
    }

    [Fact]
    public void IsActive_WhenActive_ShouldReturnTrue()
    {
        var template = CreateValidTemplate();
        var emailContent = EmailTemplateContent.Create("Subject", "Body").Value;
        template.UpdateContent(emailContent, null, null, TestUserId);
        template.Activate(TestUserId);

        template.IsActive().Should().BeTrue();
    }

    [Fact]
    public void IsActive_WhenArchived_ShouldReturnFalse()
    {
        var template = CreateValidTemplate();
        template.Archive(TestUserId);

        template.IsActive().Should().BeFalse();
    }

    [Fact]
    public void SupportsChannel_WithEnabledChannel_ShouldReturnTrue()
    {
        var template = CreateValidTemplate();

        template.SupportsChannel(NotificationChannel.Email).Should().BeTrue();
    }

    [Fact]
    public void SupportsChannel_WithDisabledChannel_ShouldReturnFalse()
    {
        var template = CreateValidTemplate();

        template.SupportsChannel(NotificationChannel.Push).Should().BeFalse();
        template.SupportsChannel(NotificationChannel.InApp).Should().BeFalse();
    }

    [Fact]
    public void HasCompleteContent_WithAllContentConfigured_ShouldReturnTrue()
    {
        var template = CreateTemplateWithAllChannels();
        var emailContent = EmailTemplateContent.Create("Subject", "Body").Value;
        var pushContent = PushTemplateContent.Create("Title", "Body").Value;
        var inAppContent = InAppTemplateContent.Create("Title", "Body").Value;
        template.UpdateContent(emailContent, pushContent, inAppContent, TestUserId);

        template.HasCompleteContent().Should().BeTrue();
    }

    [Fact]
    public void HasCompleteContent_WithMissingContent_ShouldReturnFalse()
    {
        var template = CreateTemplateWithAllChannels();
        var emailContent = EmailTemplateContent.Create("Subject", "Body").Value;
        template.UpdateContent(emailContent, null, null, TestUserId); // Missing Push and InApp

        template.HasCompleteContent().Should().BeFalse();
    }

    #endregion
}
