using FluentAssertions;
using NSubstitute;
using UniHub.Learning.Application.Abstractions;
using UniHub.Learning.Application.Commands.UploadDocument;
using UniHub.Learning.Domain.Documents;
using UniHub.SharedKernel.Results;using Xunit;
namespace UniHub.Learning.Application.Tests.Commands.UploadDocument;

public class UploadDocumentCommandHandlerTests
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IVirusScanService _virusScanService;
    private readonly UploadDocumentCommandHandler _handler;

    public UploadDocumentCommandHandlerTests()
    {
        _documentRepository = Substitute.For<IDocumentRepository>();
        _fileStorageService = Substitute.For<IFileStorageService>();
        _virusScanService = Substitute.For<IVirusScanService>();
        _handler = new UploadDocumentCommandHandler(
            _documentRepository,
            _fileStorageService,
            _virusScanService);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccessWithDocumentId()
    {
        // Arrange
        var uploaderId = Guid.NewGuid();
        var command = new UploadDocumentCommand(
            Title: "Introduction to Algorithms",
            Description: "Lecture notes for CS401",
            FileName: "algorithms.pdf",
            FileContent: new byte[1024],
            ContentType: "application/pdf",
            FileSize: 1024,
            DocumentType: DocumentType.Slide, // Slide
            UploaderId: uploaderId,
            CourseId: Guid.NewGuid());

        _virusScanService.ScanAsync(
            Arg.Any<byte[]>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns(new VirusScanResult(IsClean: true, Status: "Clean"));

        _fileStorageService.UploadFileAsync(
            Arg.Any<string>(),
            Arg.Any<byte[]>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns("/uploads/documents/algorithms.pdf");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        
        await _virusScanService.Received(1).ScanAsync(
            Arg.Any<byte[]>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());

        await _fileStorageService.Received(1).UploadFileAsync(
            Arg.Any<string>(),
            Arg.Any<byte[]>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());

        await _documentRepository.Received(1).AddAsync(
            Arg.Any<Document>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenVirusDetected_ShouldReturnFailure()
    {
        // Arrange
        var command = new UploadDocumentCommand(
            Title: "Malicious File",
            Description: "Contains virus",
            FileName: "malware.exe",
            FileContent: new byte[1024],
            ContentType: "application/pdf",
            FileSize: 1024,
            DocumentType: DocumentType.Slide,
            UploaderId: Guid.NewGuid());

        _virusScanService.ScanAsync(
            Arg.Any<byte[]>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns(new VirusScanResult(
                IsClean: false,
                Status: "Infected",
                Details: "Trojan detected"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Document.VirusDetected");
        result.Error.Message.Should().Contain("malicious");

        await _fileStorageService.DidNotReceive().UploadFileAsync(
            Arg.Any<string>(),
            Arg.Any<byte[]>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());

        await _documentRepository.DidNotReceive().AddAsync(
            Arg.Any<Document>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidTitle_ShouldCleanupFileAndReturnFailure()
    {
        // Arrange
        var command = new UploadDocumentCommand(
            Title: "ABC", // Too short
            Description: "Description",
            FileName: "file.pdf",
            FileContent: new byte[1024],
            ContentType: "application/pdf",
            FileSize: 1024,
            DocumentType: DocumentType.Slide,
            UploaderId: Guid.NewGuid());

        var uploadedFilePath = "/uploads/documents/file.pdf";

        _virusScanService.ScanAsync(
            Arg.Any<byte[]>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns(new VirusScanResult(IsClean: true, Status: "Clean"));

        _fileStorageService.UploadFileAsync(
            Arg.Any<string>(),
            Arg.Any<byte[]>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns(uploadedFilePath);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        
        // Should cleanup uploaded file
        await _fileStorageService.Received(1).DeleteFileAsync(
            uploadedFilePath,
            Arg.Any<CancellationToken>());

        await _documentRepository.DidNotReceive().AddAsync(
            Arg.Any<Document>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidFileSize_ShouldCleanupFileAndReturnFailure()
    {
        // Arrange
        var command = new UploadDocumentCommand(
            Title: "Valid Title",
            Description: "Description",
            FileName: "largefile.pdf",
            FileContent: new byte[1024],
            ContentType: "application/pdf",
            FileSize: 51 * 1024 * 1024, // 51MB, exceeds 50MB limit
            DocumentType: DocumentType.Slide,
            UploaderId: Guid.NewGuid());

        var uploadedFilePath = "/uploads/documents/largefile.pdf";

        _virusScanService.ScanAsync(
            Arg.Any<byte[]>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns(new VirusScanResult(IsClean: true, Status: "Clean"));

        _fileStorageService.UploadFileAsync(
            Arg.Any<string>(),
            Arg.Any<byte[]>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns(uploadedFilePath);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        
        // Should cleanup uploaded file
        await _fileStorageService.Received(1).DeleteFileAsync(
            uploadedFilePath,
            Arg.Any<CancellationToken>());

        await _documentRepository.DidNotReceive().AddAsync(
            Arg.Any<Document>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldScanFileBeforeUpload()
    {
        // Arrange
        var command = new UploadDocumentCommand(
            Title: "Valid Title",
            Description: "Description",
            FileName: "file.pdf",
            FileContent: new byte[1024],
            ContentType: "application/pdf",
            FileSize: 1024,
            DocumentType: DocumentType.Slide,
            UploaderId: Guid.NewGuid());

        _virusScanService.ScanAsync(
            Arg.Any<byte[]>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns(new VirusScanResult(IsClean: true, Status: "Clean"));

        _fileStorageService.UploadFileAsync(
            Arg.Any<string>(),
            Arg.Any<byte[]>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns("/uploads/file.pdf");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert - Virus scan should be called before file upload
        Received.InOrder(() =>
        {
            _virusScanService.ScanAsync(
                Arg.Any<byte[]>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());

            _fileStorageService.UploadFileAsync(
                Arg.Any<string>(),
                Arg.Any<byte[]>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());
        });
    }
}
