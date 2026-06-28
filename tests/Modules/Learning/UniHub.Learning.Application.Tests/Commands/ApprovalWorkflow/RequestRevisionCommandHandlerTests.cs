using FluentAssertions;
using NSubstitute;
using UniHub.Learning.Application.Abstractions;
using UniHub.Learning.Application.Commands.ApprovalWorkflow;
using UniHub.Learning.Domain.Documents;
using UniHub.Learning.Domain.Documents.ValueObjects;
using UniHub.SharedKernel;
using Xunit;

namespace UniHub.Learning.Application.Tests.Commands.ApprovalWorkflow;

public class RequestRevisionCommandHandlerTests
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IModeratorPermissionService _permissionService;
    private readonly RequestRevisionCommandHandler _handler;

    public RequestRevisionCommandHandlerTests()
    {
        _documentRepository = Substitute.For<IDocumentRepository>();
        _permissionService = Substitute.For<IModeratorPermissionService>();
        _handler = new RequestRevisionCommandHandler(_documentRepository, _permissionService);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var reviewerId = Guid.NewGuid();
        var command = new RequestRevisionCommand(documentId, reviewerId, "Please add more references");

        var document = Document.Create(
            DocumentTitle.Create("Test Document").Value,
            DocumentDescription.Create("Description").Value,
            DocumentFile.Create("file.pdf", "/path/file.pdf", 1024, "application/pdf").Value,
            DocumentType.Slide,
            Guid.NewGuid(),
            null).Value;
        
        document.SubmitForApproval();
        document.StartReview(reviewerId);

        _documentRepository.GetByIdAsync(DocumentId.Create(documentId), Arg.Any<CancellationToken>())
            .Returns(document);

        _permissionService.IsModeratorForDocumentAsync(reviewerId, documentId, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _documentRepository.Received(1).UpdateAsync(document, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentDocument_ShouldReturnFailure()
    {
        // Arrange
        var command = new RequestRevisionCommand(Guid.NewGuid(), Guid.NewGuid(), "Revision notes");

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
    public async Task Handle_WithUnauthorizedModerator_ShouldReturnFailure()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var reviewerId = Guid.NewGuid();
        var command = new RequestRevisionCommand(documentId, reviewerId, "Revision notes");

        var document = Document.Create(
            DocumentTitle.Create("Test Document").Value,
            DocumentDescription.Create("Description").Value,
            DocumentFile.Create("file.pdf", "/path/file.pdf", 1024, "application/pdf").Value,
            DocumentType.Slide,
            Guid.NewGuid(),
            null).Value;

        _documentRepository.GetByIdAsync(DocumentId.Create(documentId), Arg.Any<CancellationToken>())
            .Returns(document);

        _permissionService.IsModeratorForDocumentAsync(reviewerId, documentId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Document.Unauthorized");
        await _documentRepository.DidNotReceive().UpdateAsync(Arg.Any<Document>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidStatus_ShouldReturnFailure()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var reviewerId = Guid.NewGuid();
        var command = new RequestRevisionCommand(documentId, reviewerId, "Revision notes");

        // Create document in Draft status (not under review)
        var document = Document.Create(
            DocumentTitle.Create("Test Document").Value,
            DocumentDescription.Create("Description").Value,
            DocumentFile.Create("file.pdf", "/path/file.pdf", 1024, "application/pdf").Value,
            DocumentType.Slide,
            Guid.NewGuid(),
            null).Value;

        _documentRepository.GetByIdAsync(DocumentId.Create(documentId), Arg.Any<CancellationToken>())
            .Returns(document);

        _permissionService.IsModeratorForDocumentAsync(reviewerId, documentId, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        await _documentRepository.DidNotReceive().UpdateAsync(Arg.Any<Document>(), Arg.Any<CancellationToken>());
    }
}
