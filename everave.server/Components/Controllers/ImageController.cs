using everave.server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace everave.server.Components.Controllers
{

    [Route("[controller]")]
    [ApiController]
    public class ImageController(IImageStorageService imageStorageService) : ControllerBase
    {
        [HttpPost("upload")]
        [Authorize]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile upload)
        {
            if (upload == null || upload.Length == 0)
                return BadRequest("Upload failed");

            await using var resizedStream = await ImageProcessingHelper
                .ResizeImageAsync(upload.OpenReadStream(), 1200, 800).ConfigureAwait(false);

            var fileName = $"{Path.GetFileNameWithoutExtension(upload.FileName)}_{Guid.NewGuid()}{Path.GetExtension(upload.FileName)}";
            var success = await imageStorageService.UploadImageAsync(resizedStream, fileName).ConfigureAwait(false);

            return success 
                ? Ok(new { url = $"/image/{fileName}" })
                : StatusCode(500, "Failed to upload the image to the image handler");
        }

        [HttpGet("{fileName}")]
        public async Task<IActionResult> GetImage(string fileName)
        {
            var content = await imageStorageService.GetImageAsync(fileName).ConfigureAwait(false);
            return File(content, "application/octet-stream");
        }
    }
}
