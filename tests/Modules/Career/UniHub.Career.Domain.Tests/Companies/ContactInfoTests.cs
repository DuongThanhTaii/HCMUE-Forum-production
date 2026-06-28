using FluentAssertions;
using UniHub.Career.Domain.Companies;

namespace UniHub.Career.Domain.Tests.Companies;

public class ContactInfoTests
{
    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        var result = ContactInfo.Create("contact@example.com", "+84123456789", "123 Nguyen Hue St");

        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be("contact@example.com");
        result.Value.Phone.Should().Be("+84123456789");
        result.Value.Address.Should().Be("123 Nguyen Hue St");
    }

    [Fact]
    public void Create_WithEmailOnly_ShouldReturnSuccess()
    {
        var result = ContactInfo.Create("test@company.com");

        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be("test@company.com");
        result.Value.Phone.Should().BeNull();
        result.Value.Address.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Create_WithEmptyEmail_ShouldReturnFailure(string? email)
    {
        var result = ContactInfo.Create(email!);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ContactInfo.EmptyEmail");
    }

    [Fact]
    public void Create_WithEmailTooLong_ShouldReturnFailure()
    {
        var longEmail = new string('a', ContactInfo.MaxEmailLength) + "@test.com";
        var result = ContactInfo.Create(longEmail);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ContactInfo.EmailTooLong");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@test.com")]
    [InlineData("test@")]
    [InlineData("test@@test.com")]
    public void Create_WithInvalidEmailFormat_ShouldReturnFailure(string invalidEmail)
    {
        var result = ContactInfo.Create(invalidEmail);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ContactInfo.InvalidEmail");
    }

    [Fact]
    public void Create_WithPhoneTooLong_ShouldReturnFailure()
    {
        var longPhone = new string('1', ContactInfo.MaxPhoneLength + 1);
        var result = ContactInfo.Create("test@test.com", longPhone);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ContactInfo.PhoneTooLong");
    }

    [Fact]
    public void Create_WithAddressTooLong_ShouldReturnFailure()
    {
        var longAddress = new string('A', ContactInfo.MaxAddressLength + 1);
        var result = ContactInfo.Create("test@test.com", null, longAddress);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ContactInfo.AddressTooLong");
    }

    [Fact]
    public void Create_NormalizesEmailToLowerCase()
    {
        var result = ContactInfo.Create("Test@EXAMPLE.COM");
        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be("test@example.com");
    }

    [Fact]
    public void Create_TrimsWhitespace()
    {
        var result = ContactInfo.Create("  test@test.com  ", "  +84123  ", "  123 Street  ");
        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be("test@test.com");
        result.Value.Phone.Should().Be("+84123");
        result.Value.Address.Should().Be("123 Street");
    }

    [Fact]
    public void ToString_WithPhone_ShouldFormatBoth()
    {
        var contact = ContactInfo.Create("test@test.com", "+84123").Value;
        contact.ToString().Should().Be("test@test.com | +84123");
    }

    [Fact]
    public void ToString_WithoutPhone_ShouldReturnEmailOnly()
    {
        var contact = ContactInfo.Create("test@test.com").Value;
        contact.ToString().Should().Be("test@test.com");
    }

    [Fact]
    public void Equality_SameContactInfo_ShouldBeEqual()
    {
        var c1 = ContactInfo.Create("test@test.com", "+84123").Value;
        var c2 = ContactInfo.Create("test@test.com", "+84123").Value;
        c1.Should().Be(c2);
    }

    [Fact]
    public void Equality_DifferentContactInfo_ShouldNotBeEqual()
    {
        var c1 = ContactInfo.Create("test@test.com", "+84123").Value;
        var c2 = ContactInfo.Create("test@test.com", "+84999").Value;
        c1.Should().NotBe(c2);
    }
}
