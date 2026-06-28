using FluentAssertions;
using UniHub.Career.Domain.JobPostings;

namespace UniHub.Career.Domain.Tests.JobPostings;

public class SalaryRangeTests
{
    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        // Act
        var result = SalaryRange.Create(10_000_000, 20_000_000, "VND", "month");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.MinAmount.Should().Be(10_000_000);
        result.Value.MaxAmount.Should().Be(20_000_000);
        result.Value.Currency.Should().Be("VND");
        result.Value.Period.Should().Be("month");
    }

    [Fact]
    public void Create_WithNegativeMin_ShouldReturnFailure()
    {
        var result = SalaryRange.Create(-100, 20_000_000, "VND", "month");
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("SalaryRange.NegativeMin");
    }

    [Fact]
    public void Create_WithNegativeMax_ShouldReturnFailure()
    {
        var result = SalaryRange.Create(0, -100, "VND", "month");
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("SalaryRange.NegativeMax");
    }

    [Fact]
    public void Create_WithMinGreaterThanMax_ShouldReturnFailure()
    {
        var result = SalaryRange.Create(20_000_000, 10_000_000, "VND", "month");
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("SalaryRange.InvalidRange");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Create_WithEmptyCurrency_ShouldReturnFailure(string? currency)
    {
        var result = SalaryRange.Create(10_000_000, 20_000_000, currency!, "month");
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("SalaryRange.EmptyCurrency");
    }

    [Fact]
    public void Create_WithUnsupportedCurrency_ShouldReturnFailure()
    {
        var result = SalaryRange.Create(10_000_000, 20_000_000, "BTC", "month");
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("SalaryRange.UnsupportedCurrency");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Create_WithEmptyPeriod_ShouldReturnFailure(string? period)
    {
        var result = SalaryRange.Create(10_000_000, 20_000_000, "VND", period!);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("SalaryRange.EmptyPeriod");
    }

    [Fact]
    public void Create_WithUnsupportedPeriod_ShouldReturnFailure()
    {
        var result = SalaryRange.Create(10_000_000, 20_000_000, "VND", "biweekly");
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("SalaryRange.UnsupportedPeriod");
    }

    [Fact]
    public void Create_NormalizesCurrencyToUpper_AndPeriodToLower()
    {
        var result = SalaryRange.Create(10_000_000, 20_000_000, "vnd", "Month");
        result.IsSuccess.Should().BeTrue();
        result.Value.Currency.Should().Be("VND");
        result.Value.Period.Should().Be("month");
    }

    [Theory]
    [InlineData("VND")]
    [InlineData("USD")]
    [InlineData("EUR")]
    [InlineData("GBP")]
    [InlineData("JPY")]
    [InlineData("SGD")]
    [InlineData("AUD")]
    public void Create_WithAllSupportedCurrencies_ShouldSucceed(string currency)
    {
        var result = SalaryRange.Create(0, 100, currency, "month");
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData("hour")]
    [InlineData("day")]
    [InlineData("week")]
    [InlineData("month")]
    [InlineData("year")]
    public void Create_WithAllSupportedPeriods_ShouldSucceed(string period)
    {
        var result = SalaryRange.Create(0, 100, "VND", period);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_WithZeroRange_ShouldSucceed()
    {
        var result = SalaryRange.Create(0, 0, "VND", "month");
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_WithEqualMinMax_ShouldSucceed()
    {
        var result = SalaryRange.Create(15_000_000, 15_000_000, "VND", "month");
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        var salary = SalaryRange.Create(10_000_000, 20_000_000, "VND", "month").Value;
        salary.ToString().Should().Contain("VND/month");
    }

    [Fact]
    public void Equality_SameSalaryRanges_ShouldBeEqual()
    {
        var s1 = SalaryRange.Create(10_000_000, 20_000_000, "VND", "month").Value;
        var s2 = SalaryRange.Create(10_000_000, 20_000_000, "VND", "month").Value;
        s1.Should().Be(s2);
    }

    [Fact]
    public void Equality_DifferentSalaryRanges_ShouldNotBeEqual()
    {
        var s1 = SalaryRange.Create(10_000_000, 20_000_000, "VND", "month").Value;
        var s2 = SalaryRange.Create(10_000_000, 25_000_000, "VND", "month").Value;
        s1.Should().NotBe(s2);
    }
}
