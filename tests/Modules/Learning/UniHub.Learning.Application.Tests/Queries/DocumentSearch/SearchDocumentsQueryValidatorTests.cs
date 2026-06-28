using FluentAssertions;
using UniHub.Learning.Application.Queries.DocumentSearch;
using Xunit;

namespace UniHub.Learning.Application.Tests.Queries.DocumentSearch;

public class SearchDocumentsQueryValidatorTests
{
    private readonly SearchDocumentsQueryValidator _validator = new();

    [Fact]
    public void Validate_WithValidQuery_ShouldPass()
    {
        // Arrange
        var query = new SearchDocumentsQuery(
            SearchTerm: "test",
            CourseId: Guid.NewGuid(),
            FacultyId: Guid.NewGuid(),
            DocumentType: 0,
            Status: 2,
            SortBy: DocumentSortBy.Rating,
            SortDescending: true,
            PageNumber: 1,
            PageSize: 20);

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithNullSearchTerm_ShouldPass()
    {
        // Arrange
        var query = new SearchDocumentsQuery(SearchTerm: null);

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithTooLongSearchTerm_ShouldFail()
    {
        // Arrange
        var query = new SearchDocumentsQuery(SearchTerm: new string('a', 201));

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("Search term cannot exceed 200 characters");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Validate_WithInvalidPageNumber_ShouldFail(int pageNumber)
    {
        // Arrange
        var query = new SearchDocumentsQuery(PageNumber: pageNumber);

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("Page number must be greater than 0");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    [InlineData(200)]
    public void Validate_WithInvalidPageSize_ShouldFail(int pageSize)
    {
        // Arrange
        var query = new SearchDocumentsQuery(PageSize: pageSize);

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("Page size must be between 1 and 100");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(100)]
    public void Validate_WithValidPageSize_ShouldPass(int pageSize)
    {
        // Arrange
        var query = new SearchDocumentsQuery(PageSize: pageSize);

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(6)]
    [InlineData(10)]
    public void Validate_WithInvalidDocumentType_ShouldFail(int documentType)
    {
        // Arrange
        var query = new SearchDocumentsQuery(DocumentType: documentType);

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("Document type must be a valid value (0-5)");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void Validate_WithValidDocumentType_ShouldPass(int documentType)
    {
        // Arrange
        var query = new SearchDocumentsQuery(DocumentType: documentType);

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(5)]
    [InlineData(10)]
    public void Validate_WithInvalidStatus_ShouldFail(int status)
    {
        // Arrange
        var query = new SearchDocumentsQuery(Status: status);

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("Status must be a valid value (0-4)");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void Validate_WithValidStatus_ShouldPass(int status)
    {
        // Arrange
        var query = new SearchDocumentsQuery(Status: status);

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithAllFilters_ShouldPass()
    {
        // Arrange
        var query = new SearchDocumentsQuery(
            SearchTerm: "Programming",
            CourseId: Guid.NewGuid(),
            FacultyId: Guid.NewGuid(),
            DocumentType: 0,
            Status: 2,
            SortBy: DocumentSortBy.Downloads,
            SortDescending: false,
            PageNumber: 2,
            PageSize: 50);

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
