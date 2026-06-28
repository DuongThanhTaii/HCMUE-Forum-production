using UniHub.Learning.Application.Abstractions;
using UniHub.Learning.Domain.Documents;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Learning.Application.Commands.DocumentDownload;

public sealed class DownloadDocumentCommandHandler : ICommandHandler<DownloadDocumentCommand>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IUserDownloadService _userDownloadService;

    public DownloadDocumentCommandHandler(
        IDocumentRepository documentRepository,
        IUserDownloadService userDownloadService)
    {
        _documentRepository = documentRepository;
        _userDownloadService = userDownloadService;
    }

    public async Task<Result> Handle(DownloadDocumentCommand request, CancellationToken cancellationToken)
    {
        // Check if user has already downloaded this document
        var hasDownloaded = await _userDownloadService.HasUserDownloadedDocumentAsync(
            request.UserId,
            request.DocumentId,
            cancellationToken);

        if (hasDownloaded)
        {
            return Result.Failure(new Error("Document.AlreadyDownloaded", "User has already downloaded this document"));
        }

        // Get document
        var document = await _documentRepository.GetByIdAsync(
            DocumentId.Create(request.DocumentId),
            cancellationToken);

        if (document is null)
        {
            return Result.Failure(new Error("Document.NotFound", "Document not found"));
        }

        // Only allow downloading approved documents
        if (document.Status != DocumentStatus.Approved)
        {
            return Result.Failure(new Error("Document.NotApproved", "Only approved documents can be downloaded"));
        }

        // Increment download count
        document.IncrementDownloadCount();

        // Save document
        await _documentRepository.UpdateAsync(document, cancellationToken);

        // Record user download
        await _userDownloadService.RecordUserDownloadAsync(
            request.UserId,
            request.DocumentId,
            cancellationToken);

        return Result.Success();
    }
}
