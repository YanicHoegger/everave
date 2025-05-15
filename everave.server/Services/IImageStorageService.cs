namespace everave.server.Services
{
    public interface IImageStorageService
    {
        Task<bool> UploadImageAsync(Stream imageStream, string fileName);
        Task<Stream> GetImageAsync(string fileName);
        Task<bool> DeleteImageAsync(string fileName);
    }
}
