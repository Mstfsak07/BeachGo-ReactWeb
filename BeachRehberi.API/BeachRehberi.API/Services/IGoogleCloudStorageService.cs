using System.IO;

namespace BeachRehberi.API.Services;

public interface IGoogleCloudStorageService
{
    Task<string> UploadPublicImageAsync(
        Stream content,
        string objectName,
        string contentType,
        CancellationToken cancellationToken = default);
}
