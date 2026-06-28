using UniHub.SharedKernel.CQRS;

namespace UniHub.Chat.Application.Commands.UploadFile;

/// <summary>
/// Command to upload a file for chat
/// </summary>
public sealed record UploadFileCommand(
    string FileName,
    Stream FileStream,
    string ContentType,
    long FileSize,
    Guid UploadedBy) : ICommand<UploadFileResult>;

/// <summary>
/// Result of file upload
/// </summary>
public sealed record UploadFileResult(
    string FileUrl,
    string FileName,
    long FileSize,
    string ContentType);
