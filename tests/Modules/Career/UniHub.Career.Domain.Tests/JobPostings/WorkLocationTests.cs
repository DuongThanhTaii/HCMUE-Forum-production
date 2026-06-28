using FluentAssertions;
using UniHub.Career.Domain.JobPostings;

namespace UniHub.Career.Domain.Tests.JobPostings;

public class WorkLocationTests
{
    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        var result = WorkLocation.Create("Hồ Chí Minh", "Quận 1", "123 Nguyễn Huệ");

        result.IsSuccess.Should().BeTrue();
        result.Value.City.Should().Be("Hồ Chí Minh");
        result.Value.District.Should().Be("Quận 1");
        result.Value.Address.Should().Be("123 Nguyễn Huệ");
        result.Value.IsRemote.Should().BeFalse();
    }

    [Fact]
    public void Create_WithRemoteFlag_ShouldReturnSuccess()
    {
        var result = WorkLocation.Create("Hà Nội", isRemote: true);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsRemote.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Create_WithEmptyCity_ShouldReturnFailure(string? city)
    {
        var result = WorkLocation.Create(city!);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("WorkLocation.EmptyCity");
    }

    [Fact]
    public void Create_WithCityTooLong_ShouldReturnFailure()
    {
        var longCity = new string('A', WorkLocation.MaxCityLength + 1);
        var result = WorkLocation.Create(longCity);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("WorkLocation.CityTooLong");
    }

    [Fact]
    public void Create_WithDistrictTooLong_ShouldReturnFailure()
    {
        var longDistrict = new string('A', WorkLocation.MaxDistrictLength + 1);
        var result = WorkLocation.Create("HCM", longDistrict);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("WorkLocation.DistrictTooLong");
    }

    [Fact]
    public void Create_WithAddressTooLong_ShouldReturnFailure()
    {
        var longAddress = new string('A', WorkLocation.MaxAddressLength + 1);
        var result = WorkLocation.Create("HCM", "Q1", longAddress);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("WorkLocation.AddressTooLong");
    }

    [Fact]
    public void Create_WithCityOnly_ShouldReturnSuccess()
    {
        var result = WorkLocation.Create("Đà Nẵng");
        result.IsSuccess.Should().BeTrue();
        result.Value.District.Should().BeNull();
        result.Value.Address.Should().BeNull();
    }

    [Fact]
    public void Create_TrimsInput()
    {
        var result = WorkLocation.Create("  Hồ Chí Minh  ", "  Quận 1  ");
        result.IsSuccess.Should().BeTrue();
        result.Value.City.Should().Be("Hồ Chí Minh");
        result.Value.District.Should().Be("Quận 1");
    }

    [Fact]
    public void Remote_ShouldCreateRemoteLocation()
    {
        var location = WorkLocation.Remote();
        location.City.Should().Be("Remote");
        location.IsRemote.Should().BeTrue();
    }

    [Fact]
    public void ToString_WithDistrictAndCity_ShouldFormat()
    {
        var location = WorkLocation.Create("Hồ Chí Minh", "Quận 1").Value;
        location.ToString().Should().Be("Quận 1, Hồ Chí Minh");
    }

    [Fact]
    public void ToString_WithCityOnly_ShouldReturnCity()
    {
        var location = WorkLocation.Create("Hà Nội").Value;
        location.ToString().Should().Be("Hà Nội");
    }

    [Fact]
    public void ToString_Remote_ShouldAppendRemote()
    {
        var location = WorkLocation.Create("Hồ Chí Minh", isRemote: true).Value;
        location.ToString().Should().Be("Hồ Chí Minh (Remote)");
    }

    [Fact]
    public void ToString_RemoteOnly_ShouldReturnRemote()
    {
        var location = WorkLocation.Remote();
        location.ToString().Should().Be("Remote");
    }

    [Fact]
    public void Equality_SameLocations_ShouldBeEqual()
    {
        var l1 = WorkLocation.Create("HCM", "Q1").Value;
        var l2 = WorkLocation.Create("HCM", "Q1").Value;
        l1.Should().Be(l2);
    }

    [Fact]
    public void Equality_DifferentLocations_ShouldNotBeEqual()
    {
        var l1 = WorkLocation.Create("HCM", "Q1").Value;
        var l2 = WorkLocation.Create("HCM", "Q3").Value;
        l1.Should().NotBe(l2);
    }
}
