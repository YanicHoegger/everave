using Azure.Storage.Blobs;

namespace everave.server.Services
{
    public class AzureBlobStorageService(BlobServiceClient blobServiceClient) : IImageStorageService
    {
        private const string ContainerName = "images";

        public async Task<bool> UploadImageAsync(Stream imageStream, string fileName)
        {
            var containerClient = blobServiceClient.GetBlobContainerClient(ContainerName);
            await containerClient.CreateIfNotExistsAsync();

            var blobClient = containerClient.GetBlobClient(fileName);
            var response = await blobClient.UploadAsync(imageStream, overwrite: true);

            return response.GetRawResponse().Status == 201;
        }

        public async Task<Stream> GetImageAsync(string fileName)
        {
            var containerClient = blobServiceClient.GetBlobContainerClient(ContainerName);
            var blobClient = containerClient.GetBlobClient(fileName);

            var response = await blobClient.DownloadAsync();
            return response.Value.Content;
        }
    }
}
