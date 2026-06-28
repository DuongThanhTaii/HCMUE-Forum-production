using UniHub.Learning.Domain.Courses;

namespace UniHub.Learning.Domain.Tests.Courses;

public class CourseIdTests
{
    [Fact]
    public void CreateUnique_ShouldReturnNewGuidEachTime()
    {
        // Act
        var id1 = CourseId.CreateUnique();
        var id2 = CourseId.CreateUnique();

        // Assert
        id1.Should().NotBe(id2);
        id1.Value.Should().NotBeEmpty();
        id2.Value.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_WithValidGuid_ShouldReturnCourseId()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var id = CourseId.Create(guid);

        // Assert
        id.Value.Should().Be(guid);
    }

    [Fact]
    public void ToString_ShouldReturnGuidString()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id = CourseId.Create(guid);

        // Act
        var result = id.ToString();

        // Assert
        result.Should().Be(guid.ToString());
    }

    [Fact]
    public void TwoCourseIds_WithSameValue_ShouldBeEqual()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id1 = CourseId.Create(guid);
        var id2 = CourseId.Create(guid);

        // Assert
        id1.Should().Be(id2);
        (id1 == id2).Should().BeTrue();
    }

    [Fact]
    public void TwoCourseIds_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var id1 = CourseId.CreateUnique();
        var id2 = CourseId.CreateUnique();

        // Assert
        id1.Should().NotBe(id2);
        (id1 != id2).Should().BeTrue();
    }
}
