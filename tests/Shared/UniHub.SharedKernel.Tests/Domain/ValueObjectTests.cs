using FluentAssertions;
using UniHub.SharedKernel.Domain;

namespace UniHub.SharedKernel.Tests.Domain;

public class ValueObjectTests
{
    private class Address : ValueObject
    {
        public string Street { get; }
        public string City { get; }
        public string ZipCode { get; }

        public Address(string street, string city, string zipCode)
        {
            Street = street;
            City = city;
            ZipCode = zipCode;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Street;
            yield return City;
            yield return ZipCode;
        }
    }

    private class Money : ValueObject
    {
        public decimal Amount { get; }
        public string Currency { get; }

        public Money(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency;
        }
    }

    [Fact]
    public void ValueObjects_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var address1 = new Address("123 Main St", "New York", "10001");
        var address2 = new Address("123 Main St", "New York", "10001");

        // Act & Assert
        address1.Should().Be(address2);
        address1.Equals(address2).Should().BeTrue();
        (address1 == address2).Should().BeTrue();
    }

    [Fact]
    public void ValueObjects_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var address1 = new Address("123 Main St", "New York", "10001");
        var address2 = new Address("456 Oak Ave", "Boston", "02101");

        // Act & Assert
        address1.Should().NotBe(address2);
        address1.Equals(address2).Should().BeFalse();
        (address1 != address2).Should().BeTrue();
    }

    [Fact]
    public void ValueObjects_WithSameValues_ShouldHaveSameHashCode()
    {
        // Arrange
        var address1 = new Address("123 Main St", "New York", "10001");
        var address2 = new Address("123 Main St", "New York", "10001");

        // Act & Assert
        address1.GetHashCode().Should().Be(address2.GetHashCode());
    }

    [Fact]
    public void ValueObjects_WithDifferentTypes_ShouldNotBeEqual()
    {
        // Arrange
        var address = new Address("123 Main St", "New York", "10001");
        var money = new Money(100m, "USD");

        // Act & Assert
        address.Equals(money).Should().BeFalse();
    }

    [Fact]
    public void ValueObject_ComparedToNull_ShouldNotBeEqual()
    {
        // Arrange
        var address = new Address("123 Main St", "New York", "10001");

        // Act & Assert
        address.Equals(null).Should().BeFalse();
        (address == null).Should().BeFalse();
        (address != null).Should().BeTrue();
    }

    [Fact]
    public void ValueObjects_WithPartiallyDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var address1 = new Address("123 Main St", "New York", "10001");
        var address2 = new Address("123 Main St", "New York", "10002"); // Different zip

        // Act & Assert
        address1.Should().NotBe(address2);
    }
}
