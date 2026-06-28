using FluentAssertions;
using NSubstitute;
using UniHub.Learning.Application.Abstractions;
using UniHub.Learning.Application.Commands.ApprovalWorkflow;
using UniHub.Learning.Domain.Documents;
using UniHub.Learning.Domain.Documents.ValueObjects;
using UniHub.SharedKernel;
using Xunit;

namespace UniHub.Learning.Application.Tests.Commands.ApprovalWorkflow;

public class RejectDocumentCommandHandlerTests
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IModeratorPermissionService _permissionService;
    private readonly RejectDocumentCommandHandler _handler;

    public RejectDocumentCommandHandlerTests()
    {
        _documentRepository = Substitute.For<IDocumentRepository>();
        _permissionService = Substitute.For<IModeratorPermissionService>();
        _handler = new RejectDocumentCommandHandler(_documentRepository, _permissionService);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var rejectorId = Guid.NewGuid();
        var command = new RejectDocumentCommand(documentId, rejectorId, "Does not meet quality standards");

        var document = Document.Create(
            DocumentTitle.Create("Test Document").Value,
            DocumentDescription.Create("Description").Value,
            DocumentFile.Create("file.pdf", "/path/file.pdf", 1024, "application/pdf").Value,
            DocumentType.Slide,
            Guid.NewGuid(),
            null).Value;
        
        document.SubmitForApproval();
        document.StartReview(rejectorId);

        _documentRepository.GetByIdAsync(DocumentId.Create(documentId), Arg.Any<CancellationToken>())
            .Returns(document);

        _permissionService.IsModeratorForDocumentAsync(rejectorId, documentId, Arg.Any<CancellationToken>())
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
        var command = new RejectDocumentCommand(Guid.NewGuid(), Guid.NewGuid(), "Invalid reason");

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
        var rejectorId = Guid.NewGuid();
        var command = new RejectDocumentCommand(documentId, rejectorId, "Invalid reason");

        var document = Document.Create(
            DocumentTitle.Create("Test Document").Value,
            DocumentDescription.Create("Description").Value,
            DocumentFile.Create("file.pdf", "/path/file.pdf", 1024, "application/pdf").Value,
            DocumentType.Slide,
            Guid.NewGuid(),
            null).Value;

        _documentRepository.GetByIdAsync(DocumentId.Create(documentId), Arg.Any<CancellationToken>())
            .Returns(document);

        _permissionService.IsModeratorForDocumentAsync(rejectorId, documentId, Arg.Any<CancellationToken>())
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
        var rejectorId = Guid.NewGuid();
        var command = new RejectDocumentCommand(documentId, rejectorId, "Invalid reason");

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

        _permissionService.IsModeratorForDocumentAsync(rejectorId, documentId, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        await _documentRepository.DidNotReceive().UpdateAsync(Arg.Any<Document>(), Arg.Any<CancellationToken>());
    }
}
