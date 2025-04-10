using Microsoft.AspNetCore.Mvc;

namespace everave.server.Components.Controllers
{
    [Route("upload/[controller]")]
    [ApiController]
    public class ImageController(IWebHostEnvironment env) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile upload)
        {
            if (upload == null || upload.Length == 0)
                return BadRequest("Upload failed");

            var uploadsFolder = Path.Combine(env.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(upload.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            await using (var stream = System.IO.File.Create(filePath))
            {
                await upload.CopyToAsync(stream);
            }

            var url = $"{Request.Scheme}://{Request.Host}/uploads/{fileName}";

            return Ok(new { url });
        }
    }
}
