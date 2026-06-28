using UniHub.Learning.Application.Abstractions;
using UniHub.Learning.Domain.Documents;
using UniHub.Learning.Domain.Documents.ValueObjects;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Learning.Application.Commands.UploadDocument;

/// <summary>
/// Handler for uploading a new document
/// </summary>
public sealed class UploadDocumentCommandHandler : ICommandHandler<UploadDocumentCommand, Guid>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IVirusScanService _virusScanService;

    public UploadDocumentCommandHandler(
        IDocumentRepository documentRepository,
        IFileStorageService fileStorageService,
        IVirusScanService virusScanService)
    {
        _documentRepository = documentRepository;
        _fileStorageService = fileStorageService;
        _virusScanService = virusScanService;
    }

    public async Task<Result<Guid>> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
    {
        // Scan file for viruses
        var scanResult = await _virusScanService.ScanAsync(
            request.FileContent,
            request.FileName,
            cancellationToken);

        if (!scanResult.IsClean)
        {
            return Result.Failure<Guid>(
                new Error("Document.VirusDetected", 
                $"File contains malicious content: {scanResult.Details}"));
        }

        // Upload file to storage
        var filePath = await _fileStorageService.UploadFileAsync(
            request.FileName,
            request.FileContent,
            request.ContentType,
            cancellationToken);

        // Create title value object
        var titleResult = DocumentTitle.Create(request.Title);
        if (titleResult.IsFailure)
        {
            // Cleanup uploaded file on failure
            await _fileStorageService.DeleteFileAsync(filePath, cancellationToken);
            return Result.Failure<Guid>(titleResult.Error);
        }

        // Create description value object
        var descriptionResult = DocumentDescription.Create(request.Description);
        if (descriptionResult.IsFailure)
        {
            await _fileStorageService.DeleteFileAsync(filePath, cancellationToken);
            return Result.Failure<Guid>(descriptionResult.Error);
        }

        // Create file value object
        var fileResult = DocumentFile.Create(
            request.FileName,
            filePath,
            request.FileSize,
            request.ContentType);

        if (fileResult.IsFailure)
        {
            await _fileStorageService.DeleteFileAsync(filePath, cancellationToken);
            return Result.Failure<Guid>(fileResult.Error);
        }

        // Create document aggregate
        var documentResult = Document.Create(
            titleResult.Value,
            descriptionResult.Value,
            fileResult.Value,
            request.DocumentType,
            request.UploaderId,
            request.CourseId);

        if (documentResult.IsFailure)
        {
            await _fileStorageService.DeleteFileAsync(filePath, cancellationToken);
            return Result.Failure<Guid>(documentResult.Error);
        }

        // Newly uploaded documents must enter moderation queue by default.
        var submitResult = documentResult.Value.SubmitForApproval();
        if (submitResult.IsFailure)
        {
            await _fileStorageService.DeleteFileAsync(filePath, cancellationToken);
            return Result.Failure<Guid>(submitResult.Error);
        }

        // Save document
        await _documentRepository.AddAsync(documentResult.Value, cancellationToken);

        return Result.Success(documentResult.Value.Id.Value);
    }
}
