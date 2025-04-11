using Microsoft.AspNetCore.Mvc;

namespace everave.server.Components.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ImageController(IHttpClientFactory factory) : ControllerBase
    {
        private readonly HttpClient _httpClient = factory.CreateClient("ImageService");

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile upload)
        {
            if (upload == null || upload.Length == 0)
                return BadRequest("Upload failed");

            // Forward the file to the image handler
            var formData = new MultipartFormDataContent();
            var fileContent = new StreamContent(upload.OpenReadStream());
            formData.Add(fileContent, "upload", upload.FileName);

            var response = await _httpClient.PostAsync("ImageUpload", formData);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                return Ok(new { url = result });
            }
            else
            {
                return StatusCode(500, "Failed to upload the image to the image handler");
            }
        }

        [HttpGet("{fileName}")]
        public async Task<IActionResult> GetImage(string fileName)
        {
            var response = await _httpClient.GetAsync($"/uploads/{fileName}");

            if (!response.IsSuccessStatusCode)
                return NotFound();

            var content = await response.Content.ReadAsStreamAsync();
            var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
            return File(content, contentType);
        }
    }
}
