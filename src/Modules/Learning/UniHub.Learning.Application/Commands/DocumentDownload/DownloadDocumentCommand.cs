    using UniHub.SharedKernel.CQRS;

namespace UniHub.Learning.Application.Commands.DocumentDownload;

/// <summary>
/// Command to track document download
/// </summary>
public sealed record DownloadDocumentCommand(
    Guid DocumentId,
    Guid UserId) : ICommand;
