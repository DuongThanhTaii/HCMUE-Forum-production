using Microsoft.AspNetCore.Http;

namespace UniHub.Career.Presentation.Services;

public interface ICareerLogoStorageService
{
    Task<string> UploadLogoAsync(IFormFile file, Guid actorId, CancellationToken cancellationToken);
}
