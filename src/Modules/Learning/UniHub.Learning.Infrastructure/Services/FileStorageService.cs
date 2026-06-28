using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using UniHub.Learning.Application.Abstractions;

namespace UniHub.Learning.Infrastructure.Services;

/// <summary>
/// Cloudinary implementation of IFileStorageService.
/// Files are stored in Cloudinary instead of local filesystem.
/// </summary>
internal sealed class FileStorageService : IFileStorageService
{
    private readonly Cloudinary _cloudinary;

    public FileStorageService(IOptions<CloudinarySettings> cloudinarySettings)
    {
        var settings = cloudinarySettings.Value;
        var account = new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
        _cloudinary = new Cloudinary(account);
    }

    public async Task<string> UploadFileAsync(
        string fileName, 
        byte[] fileContent, 
        string contentType, 
        CancellationToken cancellationToken = default)
    {
        using var stream = new MemoryStream(fileContent);
        
        var uploadParams = new RawUploadParams
        {
            File = new FileDescription(fileName, stream),
            PublicId = $"unihub/documents/{Guid.NewGuid()}_{Path.GetFileNameWithoutExtension(fileName)}"
        };

        // We use RawUploadParams for documents (PDF, DOCX, etc.)
        var uploadResult = await _cloudinary.UploadAsync(uploadParams, "raw", cancellationToken);
        
        if (uploadResult.Error != null)
        {
            throw new Exception($"Cloudinary upload failed: {uploadResult.Error.Message}");
        }

        return uploadResult.SecureUrl.ToString();
    }

    public async Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath)) return;

        // Note: For Cloudinary, we need the public ID to delete the file.
        // We'll extract the public ID from the URL if possible.
        // A Cloudinary URL looks like: https://res.cloudinary.com/cloudname/raw/upload/v1234567/public_id.ext
        
        try
        {
            var uri = new Uri(filePath);
            var segments = uri.Segments;
            // The public ID is usually everything after /upload/v<version>/
            var uploadIndex = Array.FindIndex(segments, s => s.Equals("upload/"));
            if (uploadIndex >= 0 && segments.Length > uploadIndex + 2)
            {
                // Skip /upload/ and /v1234567/
                var publicIdWithExt = string.Join("", segments.Skip(uploadIndex + 2));
                
                var delParams = new DelResParams
                {
                    PublicIds = [publicIdWithExt],
                    ResourceType = ResourceType.Raw
                };
                await _cloudinary.DeleteResourcesAsync(delParams, cancellationToken);
            }
        }
        catch
        {
            // Ignore parse errors or deletion errors
        }
    }

    public async Task<byte[]> GetFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        using var httpClient = new HttpClient();
        return await httpClient.GetByteArrayAsync(filePath, cancellationToken);
    }

    public async Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath)) return false;

        try
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.SendAsync(
                new HttpRequestMessage(System.Net.Http.HttpMethod.Head, filePath),
                cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
