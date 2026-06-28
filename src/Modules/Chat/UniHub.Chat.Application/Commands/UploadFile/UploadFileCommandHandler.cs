using UniHub.Chat.Application.Abstractions;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Chat.Application.Commands.UploadFile;

/// <summary>
/// Handler for uploading chat files
/// </summary>
public sealed class UploadFileCommandHandler : ICommandHandler<UploadFileCommand, UploadFileResult>
{
    private readonly IFileStorageService _fileStorageService;

    public UploadFileCommandHandler(IFileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }

    public async Task<Result<UploadFileResult>> Handle(
        UploadFileCommand request,
        CancellationToken cancellationToken)
    {
        // Upload file to storage
        var uploadResult = await _fileStorageService.UploadFileAsync(
            request.FileName,
            request.FileStream,
            request.ContentType,
            cancellationToken);

        if (uploadResult.IsFailure)
        {
            return Result.Failure<UploadFileResult>(uploadResult.Error);
        }

        var result = new UploadFileResult(
            uploadResult.Value,
            request.FileName,
            request.FileSize,
            request.ContentType);

        return Result.Success(result);
    }
}
