using FluentAssertions;
using NSubstitute;
using UniHub.Learning.Application.Abstractions;
using UniHub.Learning.Application.Queries.DocumentSearch;
using UniHub.Learning.Domain.Documents;
using UniHub.Learning.Domain.Documents.ValueObjects;
using Xunit;

namespace UniHub.Learning.Application.Tests.Queries.DocumentSearch;

public class SearchDocumentsQueryHandlerTests
{
    private readonly IDocumentRepository _documentRepository = Substitute.For<IDocumentRepository>();
    private readonly SearchDocumentsQueryHandler _handler;

    public SearchDocumentsQueryHandlerTests()
    {
        _handler = new SearchDocumentsQueryHandler(_documentRepository);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnSuccessWithResults()
    {
        // Arrange
        var query = new SearchDocumentsQuery(
            SearchTerm: "test",
            PageNumber: 1,
            PageSize: 20);

        var documents = new List<Document>
        {
            CreateDocument("Document 1"),
            CreateDocument("Document 2")
        };

        _documentRepository.SearchAsync(
            query.SearchTerm,
            query.CourseId,
            query.FacultyId,
            query.DocumentType,
            query.Status,
            query.SortBy,
            query.SortDescending,
            query.PageNumber,
            query.PageSize,
            Arg.Any<CancellationToken>())
            .Returns((documents, 2));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Documents.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(2);
        result.Value.PageNumber.Should().Be(1);
        result.Value.PageSize.Should().Be(20);
        result.Value.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithNoResults_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new SearchDocumentsQuery(SearchTerm: "nonexistent");

        _documentRepository.SearchAsync(
            Arg.Any<string?>(),
            Arg.Any<Guid?>(),
            Arg.Any<Guid?>(),
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<DocumentSortBy>(),
            Arg.Any<bool>(),
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>())
            .Returns((new List<Document>(), 0));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Documents.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
        result.Value.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithPagination_ShouldCalculateCorrectTotalPages()
    {
        // Arrange
        var query = new SearchDocumentsQuery(PageNumber: 1, PageSize: 10);

        var documents = Enumerable.Range(0, 10)
            .Select(i => CreateDocument($"Document {i}"))
            .ToList();

        _documentRepository.SearchAsync(
            Arg.Any<string?>(),
            Arg.Any<Guid?>(),
            Arg.Any<Guid?>(),
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<DocumentSortBy>(),
            Arg.Any<bool>(),
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>())
            .Returns((documents, 25));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalCount.Should().Be(25);
        result.Value.TotalPages.Should().Be(3); // 25 / 10 = 2.5 -> 3 pages
    }

    [Fact]
    public async Task Handle_WithFilters_ShouldPassFiltersToRepository()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var facultyId = Guid.NewGuid();
        var query = new SearchDocumentsQuery(
            SearchTerm: "programming",
            CourseId: courseId,
            FacultyId: facultyId,
            DocumentType: 0,
            Status: 2,
            SortBy: DocumentSortBy.Rating,
            SortDescending: false,
            PageNumber: 2,
            PageSize: 15);

        _documentRepository.SearchAsync(
            query.SearchTerm,
            query.CourseId,
            query.FacultyId,
            query.DocumentType,
            query.Status,
            query.SortBy,
            query.SortDescending,
            query.PageNumber,
            query.PageSize,
            Arg.Any<CancellationToken>())
            .Returns((new List<Document>(), 0));

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _documentRepository.Received(1).SearchAsync(
            "programming",
            courseId,
            facultyId,
            0,
            2,
            DocumentSortBy.Rating,
            false,
            2,
            15,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldMapDocumentPropertiesCorrectly()
    {
        // Arrange
        var query = new SearchDocumentsQuery();
        var document = CreateDocument("Test Document");

        _documentRepository.SearchAsync(
            Arg.Any<string?>(),
            Arg.Any<Guid?>(),
            Arg.Any<Guid?>(),
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<DocumentSortBy>(),
            Arg.Any<bool>(),
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>())
            .Returns((new List<Document> { document }, 1));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var dto = result.Value.Documents.First();
        dto.Id.Should().Be(document.Id.Value);
        dto.Title.Should().Be(document.Title.Value);
        dto.Description.Should().Be(document.Description.Value);
        dto.DocumentType.Should().Be(document.Type.ToString());
        dto.Status.Should().Be(document.Status.ToString());
        dto.FileName.Should().Be(document.File.FileName);
        dto.FileSize.Should().Be(document.File.FileSize);
        dto.ContentType.Should().Be(document.File.ContentType);
        dto.UploaderId.Should().Be(document.UploaderId);
        dto.CourseId.Should().Be(document.CourseId);
    }

    [Theory]
    [InlineData(DocumentSortBy.CreatedDate)]
    [InlineData(DocumentSortBy.Title)]
    [InlineData(DocumentSortBy.Rating)]
    [InlineData(DocumentSortBy.Downloads)]
    [InlineData(DocumentSortBy.ViewCount)]
    public async Task Handle_WithDifferentSortOptions_ShouldPassToRepository(DocumentSortBy sortBy)
    {
        // Arrange
        var query = new SearchDocumentsQuery(SortBy: sortBy, SortDescending: true);

        _documentRepository.SearchAsync(
            Arg.Any<string?>(),
            Arg.Any<Guid?>(),
            Arg.Any<Guid?>(),
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            sortBy,
            true,
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>())
            .Returns((new List<Document>(), 0));

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _documentRepository.Received(1).SearchAsync(
            Arg.Any<string?>(),
            Arg.Any<Guid?>(),
            Arg.Any<Guid?>(),
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            sortBy,
            true,
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>());
    }

    private static Document CreateDocument(string title)
    {
        return Document.Create(
            DocumentTitle.Create(title).Value,
            DocumentDescription.Create("Test Description").Value,
            DocumentFile.Create("test.pdf", "/path/test.pdf", 1024, "application/pdf").Value,
            DocumentType.Slide,
            Guid.NewGuid(),
            Guid.NewGuid()).Value;
    }
}
