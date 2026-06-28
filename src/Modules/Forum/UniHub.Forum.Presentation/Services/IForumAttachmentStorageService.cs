using Microsoft.AspNetCore.Http;

namespace UniHub.Forum.Presentation.Services;

public interface IForumAttachmentStorageService
{
    Task<IReadOnlyList<string>> UploadAsync(IReadOnlyList<IFormFile> files, Guid actorId, CancellationToken cancellationToken);
}
