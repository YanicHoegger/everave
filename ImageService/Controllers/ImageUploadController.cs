using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace ImageService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImageUploadController : ControllerBase
    {
        public async Task<IActionResult> UploadImage([FromForm] IFormFile upload)
        {
            if (upload == null || upload.Length == 0)
                return BadRequest("Upload failed");

            var path = "images";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var fileName = $"{Guid.NewGuid()}.jpg";
            var filePath = Path.Combine(path, fileName);
             
            using (var stream = upload.OpenReadStream())
            {
                using (var image = await Image.LoadAsync(stream))
                {
                    const int maxWidth = 1200;
                    const int maxHeight = 800;

                    if (image.Width > maxWidth || image.Height > maxHeight)
                    {
                        image.Mutate(x => x.Resize(new ResizeOptions
                        {
                            Size = new Size(maxWidth, maxHeight),
                            Mode = ResizeMode.Max
                        }));
                    }

                    await using var outputStream = new FileStream(filePath, FileMode.Create);
                    await image.SaveAsync(outputStream, new JpegEncoder());
                }
            }

            return Ok(fileName);
        }
    }
}
