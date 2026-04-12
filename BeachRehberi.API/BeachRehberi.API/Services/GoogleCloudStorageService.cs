using Google.Cloud.Storage.V1;

namespace BeachRehberi.API.Services;

public class GoogleCloudStorageService : IGoogleCloudStorageService
{
    private readonly IConfiguration _configuration;
    private readonly Lazy<StorageClient> _storageClient;

    public GoogleCloudStorageService(IConfiguration configuration)
    {
        _configuration = configuration;
        _storageClient = new Lazy<StorageClient>(StorageClient.Create);
    }

    public async Task<string> UploadPublicImageAsync(
        Stream content,
        string objectName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var bucketName = _configuration["Gcs:BucketName"]?.Trim();
        if (string.IsNullOrWhiteSpace(bucketName))
        {
            throw new InvalidOperationException("Gcs:BucketName yapılandırılmamış.");
        }

        var uploadOptions = new UploadObjectOptions
        {
            PredefinedAcl = PredefinedObjectAcl.PublicRead
        };

        var uploadedObject = await _storageClient.Value.UploadObjectAsync(
            bucket: bucketName,
            objectName: objectName,
            contentType: contentType,
            source: content,
            options: uploadOptions,
            cancellationToken: cancellationToken);

        if (uploadedObject == null)
        {
            throw new InvalidOperationException("Dosya Google Cloud Storage'a yüklenemedi.");
        }

        return BuildPublicUrl(bucketName, objectName);
    }

    private static string BuildPublicUrl(string bucketName, string objectName)
    {
        var encodedObjectName = string.Join(
            "/",
            objectName.Split('/', StringSplitOptions.RemoveEmptyEntries)
                .Select(Uri.EscapeDataString));

        return $"https://storage.googleapis.com/{bucketName}/{encodedObjectName}";
    }
}
