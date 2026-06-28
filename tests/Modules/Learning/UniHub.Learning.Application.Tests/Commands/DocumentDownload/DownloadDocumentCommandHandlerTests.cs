using FluentAssertions;
using NSubstitute;
using UniHub.Learning.Application.Abstractions;
using UniHub.Learning.Application.Commands.DocumentDownload;
using UniHub.Learning.Domain.Documents;
using UniHub.Learning.Domain.Documents.ValueObjects;
using Xunit;

namespace UniHub.Learning.Application.Tests.Commands.DocumentDownload;

public class DownloadDocumentCommandHandlerTests
{
    private readonly IDocumentRepository _documentRepository = Substitute.For<IDocumentRepository>();
    private readonly IUserDownloadService _userDownloadService = Substitute.For<IUserDownloadService>();
    private readonly DownloadDocumentCommandHandler _handler;

    public DownloadDocumentCommandHandlerTests()
    {
        _handler = new DownloadDocumentCommandHandler(_documentRepository, _userDownloadService);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var command = new DownloadDocumentCommand(
            DocumentId: Guid.NewGuid(),
            UserId: Guid.NewGuid());

        var document = CreateApprovedDocument();

        _userDownloadService.HasUserDownloadedDocumentAsync(
            command.UserId,
            command.DocumentId,
            Arg.Any<CancellationToken>())
            .Returns(false);

        _documentRepository.GetByIdAsync(
            Arg.Is<DocumentId>(id => id.Value == command.DocumentId),
            Arg.Any<CancellationToken>())
            .Returns(document);

        var initialDownloadCount = document.DownloadCount;

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        document.DownloadCount.Should().Be(initialDownloadCount + 1);
        
        await _documentRepository.Received(1).UpdateAsync(document, Arg.Any<CancellationToken>());
        await _userDownloadService.Received(1).RecordUserDownloadAsync(
            command.UserId,
            command.DocumentId,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserAlreadyDownloaded_ShouldReturnFailure()
    {
        // Arrange
        var command = new DownloadDocumentCommand(
            DocumentId: Guid.NewGuid(),
            UserId: Guid.NewGuid());

        _userDownloadService.HasUserDownloadedDocumentAsync(
            command.UserId,
            command.DocumentId,
            Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Document.AlreadyDownloaded");
        result.Error.Message.Should().Be("User has already downloaded this document");

        await _documentRepository.DidNotReceive().GetByIdAsync(Arg.Any<DocumentId>(), Arg.Any<CancellationToken>());
        await _documentRepository.DidNotReceive().UpdateAsync(Arg.Any<Document>(), Arg.Any<CancellationToken>());
        await _userDownloadService.DidNotReceive().RecordUserDownloadAsync(
            Arg.Any<Guid>(),
            Arg.Any<Guid>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDocumentNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new DownloadDocumentCommand(
            DocumentId: Guid.NewGuid(),
            UserId: Guid.NewGuid());

        _userDownloadService.HasUserDownloadedDocumentAsync(
            command.UserId,
            command.DocumentId,
            Arg.Any<CancellationToken>())
            .Returns(false);

        _documentRepository.GetByIdAsync(
            Arg.Is<DocumentId>(id => id.Value == command.DocumentId),
            Arg.Any<CancellationToken>())
            .Returns((Document?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Document.NotFound");
        result.Error.Message.Should().Be("Document not found");

        await _documentRepository.DidNotReceive().UpdateAsync(Arg.Any<Document>(), Arg.Any<CancellationToken>());
        await _userDownloadService.DidNotReceive().RecordUserDownloadAsync(
            Arg.Any<Guid>(),
            Arg.Any<Guid>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDocumentNotApproved_ShouldReturnFailure()
    {
        // Arrange
        var command = new DownloadDocumentCommand(
            DocumentId: Guid.NewGuid(),
            UserId: Guid.NewGuid());

        var document = CreateDraftDocument();

        _userDownloadService.HasUserDownloadedDocumentAsync(
            command.UserId,
            command.DocumentId,
            Arg.Any<CancellationToken>())
            .Returns(false);

        _documentRepository.GetByIdAsync(
            Arg.Is<DocumentId>(id => id.Value == command.DocumentId),
            Arg.Any<CancellationToken>())
            .Returns(document);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Document.NotApproved");
        result.Error.Message.Should().Be("Only approved documents can be downloaded");

        await _documentRepository.DidNotReceive().UpdateAsync(Arg.Any<Document>(), Arg.Any<CancellationToken>());
        await _userDownloadService.DidNotReceive().RecordUserDownloadAsync(
            Arg.Any<Guid>(),
            Arg.Any<Guid>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(DocumentStatus.PendingApproval)]
    [InlineData(DocumentStatus.Rejected)]
    [InlineData(DocumentStatus.Deleted)]
    public async Task Handle_WhenDocumentStatusIsNotApproved_ShouldReturnFailure(DocumentStatus status)
    {
        // Arrange
        var command = new DownloadDocumentCommand(
            DocumentId: Guid.NewGuid(),
            UserId: Guid.NewGuid());

        var document = CreateDocumentWithStatus(status);

        _userDownloadService.HasUserDownloadedDocumentAsync(
            command.UserId,
            command.DocumentId,
            Arg.Any<CancellationToken>())
            .Returns(false);

        _documentRepository.GetByIdAsync(
            Arg.Is<DocumentId>(id => id.Value == command.DocumentId),
            Arg.Any<CancellationToken>())
            .Returns(document);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Document.NotApproved");

        await _documentRepository.DidNotReceive().UpdateAsync(Arg.Any<Document>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldIncrementDownloadCountByOne()
    {
        // Arrange
        var command = new DownloadDocumentCommand(
            DocumentId: Guid.NewGuid(),
            UserId: Guid.NewGuid());

        var document = CreateApprovedDocument();
        var initialCount = document.DownloadCount;

        _userDownloadService.HasUserDownloadedDocumentAsync(
            command.UserId,
            command.DocumentId,
            Arg.Any<CancellationToken>())
            .Returns(false);

        _documentRepository.GetByIdAsync(
            Arg.Is<DocumentId>(id => id.Value == command.DocumentId),
            Arg.Any<CancellationToken>())
            .Returns(document);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        document.DownloadCount.Should().Be(initialCount + 1);
    }

    [Fact]
    public async Task Handle_ShouldCallServicesInCorrectOrder()
    {
        // Arrange
        var command = new DownloadDocumentCommand(
            DocumentId: Guid.NewGuid(),
            UserId: Guid.NewGuid());

        var document = CreateApprovedDocument();
        var callOrder = new List<string>();

        _userDownloadService.HasUserDownloadedDocumentAsync(
            command.UserId,
            command.DocumentId,
            Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                callOrder.Add("HasUserDownloadedDocumentAsync");
                return false;
            });

        _documentRepository.GetByIdAsync(
            Arg.Is<DocumentId>(id => id.Value == command.DocumentId),
            Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                callOrder.Add("GetByIdAsync");
                return document;
            });

        _documentRepository.UpdateAsync(document, Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                callOrder.Add("UpdateAsync");
                return Task.CompletedTask;
            });

        _userDownloadService.RecordUserDownloadAsync(
            command.UserId,
            command.DocumentId,
            Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                callOrder.Add("RecordUserDownloadAsync");
                return Task.CompletedTask;
            });

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        callOrder.Should().Equal(
            "HasUserDownloadedDocumentAsync",
            "GetByIdAsync",
            "UpdateAsync",
            "RecordUserDownloadAsync");
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

        document.SubmitForApproval();
        document.Approve(Guid.NewGuid(), "Approved");

        return document;
    }

    private static Document CreateDraftDocument()
    {
        return Document.Create(
            DocumentTitle.Create("Test Document").Value,
            DocumentDescription.Create("Test Description").Value,
            DocumentFile.Create("test.pdf", "/path/test.pdf", 1024, "application/pdf").Value,
            DocumentType.Slide,
            Guid.NewGuid(),
            Guid.NewGuid()).Value;
    }

    private static Document CreateDocumentWithStatus(DocumentStatus status)
    {
        var document = Document.Create(
            DocumentTitle.Create("Test Document").Value,
            DocumentDescription.Create("Test Description").Value,
            DocumentFile.Create("test.pdf", "/path/test.pdf", 1024, "application/pdf").Value,
            DocumentType.Slide,
            Guid.NewGuid(),
            Guid.NewGuid()).Value;

        switch (status)
        {
            case DocumentStatus.PendingApproval:
                document.SubmitForApproval();
                break;
            case DocumentStatus.Rejected:
                document.SubmitForApproval();
                document.Reject(Guid.NewGuid(), "Rejected for testing");
                break;
            case DocumentStatus.Deleted:
                document.Delete(Guid.NewGuid());
                break;
        }

        return document;
    }
}
