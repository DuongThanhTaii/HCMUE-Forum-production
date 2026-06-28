using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace UniHub.Career.Presentation.Services;

public sealed class CareerLogoStorageService : ICareerLogoStorageService
{
    private readonly Cloudinary _cloudinary;

    public CareerLogoStorageService(IOptions<CareerCloudinarySettings> settings)
    {
        var value = settings.Value;
        var account = new Account(value.CloudName, value.ApiKey, value.ApiSecret);
        _cloudinary = new Cloudinary(account);
    }

    public async Task<string> UploadLogoAsync(IFormFile file, Guid actorId, CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        var publicId = $"unihub/career/company-logos/{actorId}/{Guid.NewGuid()}";
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            PublicId = publicId
        };

        var result = await _cloudinary.UploadAsync(uploadParams, cancellationToken);
        if (result.Error is not null || result.SecureUrl is null)
        {
            throw new InvalidOperationException(result.Error?.Message ?? "Cloudinary company logo upload failed.");
        }

        return result.SecureUrl.ToString();
    }
}
