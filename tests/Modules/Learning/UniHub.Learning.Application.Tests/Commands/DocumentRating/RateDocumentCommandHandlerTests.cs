using FluentAssertions;
using NSubstitute;
using UniHub.Learning.Application.Abstractions;
using UniHub.Learning.Application.Commands.DocumentRating;
using UniHub.Learning.Domain.Documents;
using UniHub.Learning.Domain.Documents.ValueObjects;
using UniHub.SharedKernel.Results;
using Xunit;

namespace UniHub.Learning.Application.Tests.Commands.DocumentRating;

public class RateDocumentCommandHandlerTests
{
    private readonly IDocumentRepository _documentRepository = Substitute.For<IDocumentRepository>();
    private readonly IUserRatingService _userRatingService = Substitute.For<IUserRatingService>();
    private readonly RateDocumentCommandHandler _handler;

    public RateDocumentCommandHandlerTests()
    {
        _handler = new RateDocumentCommandHandler(_documentRepository, _userRatingService);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new RateDocumentCommand(documentId, userId, 5);

        var document = CreateApprovedDocument();

        _userRatingService.HasUserRatedDocumentAsync(userId, documentId, Arg.Any<CancellationToken>())
            .Returns(false);

        _documentRepository.GetByIdAsync(Arg.Any<DocumentId>(), Arg.Any<CancellationToken>())
            .Returns(document);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _documentRepository.Received(1).UpdateAsync(document, Arg.Any<CancellationToken>());
        await _userRatingService.Received(1).RecordUserRatingAsync(userId, documentId, 5, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithUserAlreadyRated_ShouldReturnFailure()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new RateDocumentCommand(documentId, userId, 5);

        _userRatingService.HasUserRatedDocumentAsync(userId, documentId, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Document.AlreadyRated");
        await _documentRepository.DidNotReceive().GetByIdAsync(Arg.Any<DocumentId>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentDocument_ShouldReturnFailure()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new RateDocumentCommand(documentId, userId, 5);

        _userRatingService.HasUserRatedDocumentAsync(userId, documentId, Arg.Any<CancellationToken>())
            .Returns(false);

        _documentRepository.GetByIdAsync(Arg.Any<DocumentId>(), Arg.Any<CancellationToken>())
            .Returns((Document?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Document.NotFound");
        await _documentRepository.DidNotReceive().UpdateAsync(Arg.Any<Document>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNotApprovedDocument_ShouldReturnFailure()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new RateDocumentCommand(documentId, userId, 5);

        var document = CreateDraftDocument();

        _userRatingService.HasUserRatedDocumentAsync(userId, documentId, Arg.Any<CancellationToken>())
            .Returns(false);

        _documentRepository.GetByIdAsync(Arg.Any<DocumentId>(), Arg.Any<CancellationToken>())
            .Returns(document);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Document.NotApproved");
        await _documentRepository.DidNotReceive().UpdateAsync(Arg.Any<Document>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidRating_ShouldReturnFailure()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new RateDocumentCommand(documentId, userId, 10); // Invalid rating

        var document = CreateApprovedDocument();

        _userRatingService.HasUserRatedDocumentAsync(userId, documentId, Arg.Any<CancellationToken>())
            .Returns(false);

        _documentRepository.GetByIdAsync(Arg.Any<DocumentId>(), Arg.Any<CancellationToken>())
            .Returns(document);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Document.InvalidRating");
        await _documentRepository.DidNotReceive().UpdateAsync(Arg.Any<Document>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public async Task Handle_WithAllValidRatings_ShouldReturnSuccess(int rating)
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new RateDocumentCommand(documentId, userId, rating);

        var document = CreateApprovedDocument();

        _userRatingService.HasUserRatedDocumentAsync(userId, documentId, Arg.Any<CancellationToken>())
            .Returns(false);

        _documentRepository.GetByIdAsync(Arg.Any<DocumentId>(), Arg.Any<CancellationToken>())
            .Returns(document);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _documentRepository.Received(1).UpdateAsync(document, Arg.Any<CancellationToken>());
        await _userRatingService.Received(1).RecordUserRatingAsync(userId, documentId, rating, Arg.Any<CancellationToken>());
    }

    private static Document CreateApprovedDocument()
    {
        var document = Document.Create(
            DocumentTitle.Create("Test Document").Value,
            DocumentDescription.Create("Test Description").Value,
            DocumentFile.Create("test.pdf", "/path/test.pdf", 1024, "application/pdf").Value,
            DocumentType.Slide,
            Guid.NewGuid(),
            Guid.NewGuid()).Value;

        // Submit and approve the document
        document.SubmitForApproval();
        document.Approve(Guid.NewGuid(), "Approved");

        return document;
    }

    private static Document CreateDraftDocument()
    {
        var document = Document.Create(
            DocumentTitle.Create("Draft Document").Value,
            DocumentDescription.Create("Draft Description").Value,
            DocumentFile.Create("draft.pdf", "/path/draft.pdf", 1024, "application/pdf").Value,
            DocumentType.Slide,
            Guid.NewGuid(),
            Guid.NewGuid()).Value;

        // Keep in Draft status
        return document;
    }
}
