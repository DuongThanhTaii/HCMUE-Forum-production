using Microsoft.AspNetCore.Http;

namespace UniHub.Forum.Presentation.DTOs.Posts;

public sealed record UploadForumAttachmentsRequest
{
    public List<IFormFile> Files { get; init; } = [];
}
