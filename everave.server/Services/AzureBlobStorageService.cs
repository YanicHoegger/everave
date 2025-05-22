using Azure.Storage.Blobs;

namespace everave.server.Services;

public class AzureBlobStorageService(BlobServiceClient blobServiceClient) : IImageStorageService
{
    private const string ContainerName = "images";

    public async Task<bool> UploadImageAsync(Stream imageStream, string fileName)
    {
        var blobClient = GetBlobClient(fileName);
        var response = await blobClient.UploadAsync(imageStream, overwrite: true);

        return response.GetRawResponse().Status == 201;
    }

    public async Task<Stream> GetImageAsync(string fileName)
    {
        var response = await GetBlobClient(fileName).DownloadAsync();
        return response.Value.Content;
    }

    public async Task<bool> DeleteImageAsync(string fileName)
    {
        var response = await GetBlobClient(fileName).DeleteAsync();

        return response.Status == 201;
    }

    private BlobClient GetBlobClient(string fileName)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(ContainerName);
        containerClient.CreateIfNotExists();

        return containerClient.GetBlobClient(fileName);
    }
}