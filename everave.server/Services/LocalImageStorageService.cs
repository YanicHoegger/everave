using System.Net.Http.Headers;

namespace everave.server.Services
{
    public class LocalImageStorageService(IHttpClientFactory httpClientFactory) : IImageStorageService
    {
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient("ImageService");

        public async Task<bool> UploadImageAsync(Stream imageStream, string fileName)
        {
            var formData = new MultipartFormDataContent();
            var fileContent = new StreamContent(imageStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            formData.Add(fileContent, "upload", fileName);

            var response = await _httpClient.PostAsync("Image/upload", formData);

            return response.IsSuccessStatusCode;
        }

        public async Task<Stream> GetImageAsync(string fileName)
        {
            var response = await _httpClient.GetAsync($"/uploads/{fileName}");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStreamAsync();
        }

        public async Task<bool> DeleteImageAsync(string fileName)
        {
            var response = await _httpClient.PostAsync($"/Image/delete/{fileName}", null);
            return response.IsSuccessStatusCode;
        }
    }
}