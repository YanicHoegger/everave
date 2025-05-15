using Microsoft.AspNetCore.Mvc;

namespace ImageService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImageController : ControllerBase
    {
        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile upload)
        {
            if (upload == null || upload.Length == 0)
                return BadRequest("Upload failed");

            var path = "images";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var filePath = Path.Combine(path, upload.FileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await upload.CopyToAsync(stream);
            }

            return Ok();
        }

        [HttpPost("delete/{fileName}")]
        public IActionResult DeleteImage(string fileName)
        {
            var path = "images";
            var filePath = Path.Combine(path, fileName);

            if (!System.IO.File.Exists(filePath)) 
                return NotFound("File not found");

            System.IO.File.Delete(filePath);

            return Ok();
        }
    }
}
