using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using UserAPI.AzureServices;

public class BlobService : IBlobService
{
    private readonly string _connectionString;
    private readonly string _containerName;

    public BlobService(IConfiguration config)
    {
        _connectionString = config["AzureStorage:ConnectionString"];
        _containerName = config["AzureStorage:ContainerName"];
    }

    public async Task<string> UploadAsync(IFormFile file, string fileName)
    {
        var container = new BlobContainerClient(_connectionString, _containerName);
        //await container.CreateIfNotExistsAsync(PublicAccessType.Blob); // ✅ FIXED

        var blob = container.GetBlobClient(fileName);

        await using var stream = file.OpenReadStream();
        await blob.UploadAsync(stream, overwrite: true);

        return blob.Uri.ToString();
    }
}
