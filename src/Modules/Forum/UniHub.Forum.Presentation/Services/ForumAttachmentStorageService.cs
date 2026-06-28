using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace UniHub.Forum.Presentation.Services;

public sealed class ForumAttachmentStorageService : IForumAttachmentStorageService
{
    private readonly Cloudinary _cloudinary;

    public ForumAttachmentStorageService(IOptions<ForumCloudinarySettings> settings)
    {
        var value = settings.Value;
        var account = new Account(value.CloudName, value.ApiKey, value.ApiSecret);
        _cloudinary = new Cloudinary(account);
    }

    public async Task<IReadOnlyList<string>> UploadAsync(
        IReadOnlyList<IFormFile> files,
        Guid actorId,
        CancellationToken cancellationToken)
    {
        var uploadedUrls = new List<string>(files.Count);

        foreach (var file in files)
        {
            await using var stream = file.OpenReadStream();
            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            var publicId = $"unihub/forum/{actorId}/{Guid.NewGuid()}";

            var isImage = extension is ".png" or ".jpg" or ".jpeg" or ".gif" or ".webp" or ".bmp";
            if (isImage)
            {
                var imageParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    PublicId = publicId
                };
                var imageResult = await _cloudinary.UploadAsync(imageParams, cancellationToken);
                if (imageResult.Error is not null || imageResult.SecureUrl is null)
                {
                    throw new InvalidOperationException(imageResult.Error?.Message ?? "Cloudinary image upload failed.");
                }

                uploadedUrls.Add(imageResult.SecureUrl.ToString());
                continue;
            }

            var rawParams = new RawUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                PublicId = publicId
            };
            var rawResult = await _cloudinary.UploadAsync(rawParams, "raw", cancellationToken);
            if (rawResult.Error is not null || rawResult.SecureUrl is null)
            {
                throw new InvalidOperationException(rawResult.Error?.Message ?? "Cloudinary file upload failed.");
            }

            uploadedUrls.Add(rawResult.SecureUrl.ToString());
        }

        return uploadedUrls;
    }
}
