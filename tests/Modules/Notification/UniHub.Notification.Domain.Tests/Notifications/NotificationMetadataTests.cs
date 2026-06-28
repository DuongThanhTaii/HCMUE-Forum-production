using UniHub.Notification.Domain.Notifications;

namespace UniHub.Notification.Domain.Tests.Notifications;

public class NotificationMetadataTests
{
    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        var data = new Dictionary<string, string>
        {
            { "UserName", "John Doe" },
            { "JobTitle", "Software Engineer" },
            { "CompanyName", "TechCorp" }
        };

        var result = NotificationMetadata.Create(data);

        result.IsSuccess.Should().BeTrue();
        result.Value.Data.Should().HaveCount(3);
        result.Value.GetValue("UserName").Should().Be("John Doe");
        result.Value.GetValue("JobTitle").Should().Be("Software Engineer");
        result.Value.GetValue("CompanyName").Should().Be("TechCorp");
    }

    [Fact]
    public void Create_WithNullData_ShouldReturnEmptyMetadata()
    {
        var result = NotificationMetadata.Create(null);

        result.IsSuccess.Should().BeTrue();
        result.Value.Data.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithEmptyData_ShouldReturnSuccess()
    {
        var result = NotificationMetadata.Create(new Dictionary<string, string>());

        result.IsSuccess.Should().BeTrue();
        result.Value.Data.Should().BeEmpty();
    }

    [Fact]
    public void Empty_ShouldReturnEmptyMetadata()
    {
        var metadata = NotificationMetadata.Empty();

        metadata.Data.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithTooManyKeys_ShouldReturnFailure()
    {
        var data = new Dictionary<string, string>();
        for (int i = 0; i < NotificationMetadata.MaxKeys + 1; i++)
        {
            data[$"Key{i}"] = $"Value{i}";
        }

        var result = NotificationMetadata.Create(data);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NotificationMetadata.TooManyKeys");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_WithEmptyKey_ShouldReturnFailure(string emptyKey)
    {
        var data = new Dictionary<string, string>
        {
            { emptyKey, "Value" }
        };

        var result = NotificationMetadata.Create(data);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NotificationMetadata.EmptyKey");
    }

    [Fact]
    public void Create_WithKeyTooLong_ShouldReturnFailure()
    {
        var longKey = new string('K', NotificationMetadata.MaxKeyLength + 1);
        var data = new Dictionary<string, string>
        {
            { longKey, "Value" }
        };

        var result = NotificationMetadata.Create(data);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NotificationMetadata.KeyTooLong");
    }

    [Fact]
    public void Create_WithValueTooLong_ShouldReturnFailure()
    {
        var longValue = new string('V', NotificationMetadata.MaxValueLength + 1);
        var data = new Dictionary<string, string>
        {
            { "Key", longValue }
        };

        var result = NotificationMetadata.Create(data);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NotificationMetadata.ValueTooLong");
    }

    [Fact]
    public void GetValue_WithExistingKey_ShouldReturnValue()
    {
        var data = new Dictionary<string, string> { { "UserName", "John" } };
        var metadata = NotificationMetadata.Create(data).Value;

        var value = metadata.GetValue("UserName");

        value.Should().Be("John");
    }

    [Fact]
    public void GetValue_WithNonExistentKey_ShouldReturnNull()
    {
        var metadata = NotificationMetadata.Empty();

        var value = metadata.GetValue("NonExistent");

        value.Should().BeNull();
    }

    [Fact]
    public void GetValue_ShouldBeCaseInsensitive()
    {
        var data = new Dictionary<string, string> { { "UserName", "John" } };
        var metadata = NotificationMetadata.Create(data).Value;

        var value1 = metadata.GetValue("USERNAME");
        var value2 = metadata.GetValue("username");
        var value3 = metadata.GetValue("UserName");

        value1.Should().Be("John");
        value2.Should().Be("John");
        value3.Should().Be("John");
    }

    [Fact]
    public void ContainsKey_WithExistingKey_ShouldReturnTrue()
    {
        var data = new Dictionary<string, string> { { "UserName", "John" } };
        var metadata = NotificationMetadata.Create(data).Value;

        metadata.ContainsKey("UserName").Should().BeTrue();
    }

    [Fact]
    public void ContainsKey_WithNonExistentKey_ShouldReturnFalse()
    {
        var metadata = NotificationMetadata.Empty();

        metadata.ContainsKey("NonExistent").Should().BeFalse();
    }

    [Fact]
    public void ContainsKey_ShouldBeCaseInsensitive()
    {
        var data = new Dictionary<string, string> { { "UserName", "John" } };
        var metadata = NotificationMetadata.Create(data).Value;

        metadata.ContainsKey("USERNAME").Should().BeTrue();
        metadata.ContainsKey("username").Should().BeTrue();
        metadata.ContainsKey("UserName").Should().BeTrue();
    }

    [Fact]
    public void ToString_WithEmptyMetadata_ShouldReturnNoMetadataMessage()
    {
        var metadata = NotificationMetadata.Empty();

        metadata.ToString().Should().Be("No metadata");
    }

    [Fact]
    public void ToString_WithData_ShouldReturnCountMessage()
    {
        var data = new Dictionary<string, string>
        {
            { "Key1", "Value1" },
            { "Key2", "Value2" }
        };
        var metadata = NotificationMetadata.Create(data).Value;

        metadata.ToString().Should().Be("2 metadata items");
    }

    [Fact]
    public void Equals_WithSameData_ShouldReturnTrue()
    {
        var data = new Dictionary<string, string> { { "Key", "Value" } };
        var metadata1 = NotificationMetadata.Create(data).Value;
        var metadata2 = NotificationMetadata.Create(data).Value;

        metadata1.Should().Be(metadata2);
        (metadata1 == metadata2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentData_ShouldReturnFalse()
    {
        var data1 = new Dictionary<string, string> { { "Key1", "Value1" } };
        var data2 = new Dictionary<string, string> { { "Key2", "Value2" } };
        var metadata1 = NotificationMetadata.Create(data1).Value;
        var metadata2 = NotificationMetadata.Create(data2).Value;

        metadata1.Should().NotBe(metadata2);
        (metadata1 != metadata2).Should().BeTrue();
    }
}
