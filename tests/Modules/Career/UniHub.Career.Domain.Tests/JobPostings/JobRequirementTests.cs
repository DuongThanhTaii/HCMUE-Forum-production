using FluentAssertions;
using UniHub.Career.Domain.JobPostings;

namespace UniHub.Career.Domain.Tests.JobPostings;

public class JobRequirementTests
{
    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        var result = JobRequirement.Create("C#", true);

        result.IsSuccess.Should().BeTrue();
        result.Value.Skill.Should().Be("C#");
        result.Value.IsRequired.Should().BeTrue();
    }

    [Fact]
    public void Create_WithOptionalSkill_ShouldReturnSuccess()
    {
        var result = JobRequirement.Create("Docker", false);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsRequired.Should().BeFalse();
    }

    [Fact]
    public void Create_DefaultIsRequired_ShouldBeTrue()
    {
        var result = JobRequirement.Create("React");

        result.IsSuccess.Should().BeTrue();
        result.Value.IsRequired.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Create_WithEmptySkill_ShouldReturnFailure(string? skill)
    {
        var result = JobRequirement.Create(skill!);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("JobRequirement.EmptySkill");
    }

    [Fact]
    public void Create_WithSkillTooLong_ShouldReturnFailure()
    {
        var longSkill = new string('A', JobRequirement.MaxSkillLength + 1);
        var result = JobRequirement.Create(longSkill);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("JobRequirement.SkillTooLong");
    }

    [Fact]
    public void Create_TrimsSkillName()
    {
        var result = JobRequirement.Create("  C#  ");
        result.IsSuccess.Should().BeTrue();
        result.Value.Skill.Should().Be("C#");
    }

    [Fact]
    public void ToString_Required_ShouldShowRequired()
    {
        var req = JobRequirement.Create("C#", true).Value;
        req.ToString().Should().Be("C# (required)");
    }

    [Fact]
    public void ToString_Preferred_ShouldShowPreferred()
    {
        var req = JobRequirement.Create("Docker", false).Value;
        req.ToString().Should().Be("Docker (preferred)");
    }

    [Fact]
    public void Equality_SameRequirements_ShouldBeEqual()
    {
        var r1 = JobRequirement.Create("C#", true).Value;
        var r2 = JobRequirement.Create("C#", true).Value;
        r1.Should().Be(r2);
    }

    [Fact]
    public void Equality_DifferentSkills_ShouldNotBeEqual()
    {
        var r1 = JobRequirement.Create("C#", true).Value;
        var r2 = JobRequirement.Create("Java", true).Value;
        r1.Should().NotBe(r2);
    }

    [Fact]
    public void Equality_DifferentRequiredFlag_ShouldNotBeEqual()
    {
        var r1 = JobRequirement.Create("C#", true).Value;
        var r2 = JobRequirement.Create("C#", false).Value;
        r1.Should().NotBe(r2);
    }
}
