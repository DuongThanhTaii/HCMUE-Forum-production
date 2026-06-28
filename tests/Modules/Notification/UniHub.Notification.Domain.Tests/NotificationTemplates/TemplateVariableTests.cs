using UniHub.Notification.Domain.NotificationTemplates;

namespace UniHub.Notification.Domain.Tests.NotificationTemplates;

public class TemplateVariableTests
{
    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        var result = TemplateVariable.Create(
            "UserName",
            "The name of the user",
            "John Doe");

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("UserName");
        result.Value.Description.Should().Be("The name of the user");
        result.Value.ExampleValue.Should().Be("John Doe");
    }

    [Fact]
    public void Create_WithNameAndDescriptionOnly_ShouldReturnSuccess()
    {
        var result = TemplateVariable.Create("PostTitle", "The title of the post");

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("PostTitle");
        result.Value.Description.Should().Be("The title of the post");
        result.Value.ExampleValue.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Create_WithEmptyName_ShouldReturnFailure(string? name)
    {
        var result = TemplateVariable.Create(name!, "Description");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TemplateVariable.EmptyName");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Create_WithEmptyDescription_ShouldReturnFailure(string? description)
    {
        var result = TemplateVariable.Create("Name", description!);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TemplateVariable.EmptyDescription");
    }

    [Theory]
    [InlineData("User Name")]
    [InlineData("User-Name")]
    [InlineData("User@Name")]
    [InlineData("User.Name")]
    public void Create_WithInvalidNameFormat_ShouldReturnFailure(string invalidName)
    {
        var result = TemplateVariable.Create(invalidName, "Description");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TemplateVariable.InvalidNameFormat");
    }

    [Theory]
    [InlineData("UserName")]
    [InlineData("User_Name")]
    [InlineData("USER_NAME")]
    [InlineData("userName123")]
    [InlineData("_privateName")]
    public void Create_WithValidNameFormat_ShouldReturnSuccess(string validName)
    {
        var result = TemplateVariable.Create(validName, "Description");

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(validName);
    }

    [Fact]
    public void Create_WithNameTooLong_ShouldReturnFailure()
    {
        var longName = new string('N', TemplateVariable.MaxNameLength + 1);
        var result = TemplateVariable.Create(longName, "Description");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TemplateVariable.NameTooLong");
    }

    [Fact]
    public void Create_WithDescriptionTooLong_ShouldReturnFailure()
    {
        var longDescription = new string('D', TemplateVariable.MaxDescriptionLength + 1);
        var result = TemplateVariable.Create("Name", longDescription);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TemplateVariable.DescriptionTooLong");
    }

    [Fact]
    public void Create_WithExampleTooLong_ShouldReturnFailure()
    {
        var longExample = new string('E', TemplateVariable.MaxExampleLength + 1);
        var result = TemplateVariable.Create("Name", "Description", longExample);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TemplateVariable.ExampleTooLong");
    }

    [Fact]
    public void Create_TrimsWhitespace()
    {
        var result = TemplateVariable.Create(
            "  UserName  ",
            "  The user name  ",
            "  Example  ");

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("UserName");
        result.Value.Description.Should().Be("The user name");
        result.Value.ExampleValue.Should().Be("Example");
    }

    [Fact]
    public void ToPlaceholder_ShouldReturnVariableInBraces()
    {
        var variable = TemplateVariable.Create("UserName", "Description").Value;

        variable.ToPlaceholder().Should().Be("{UserName}");
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        var variable = TemplateVariable.Create("UserName", "The user name").Value;

        variable.ToString().Should().Be("{UserName} - The user name");
    }
}
