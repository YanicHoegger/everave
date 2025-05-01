using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace everave.server.Components.Controllers
{
    public static class ImageProcessingHelper
    {
        public static async Task<Stream> ResizeImageAsync(Stream inputStream, int maxWidth, int maxHeight)
        {
            var outputStream = new MemoryStream();

            using (var image = await Image.LoadAsync(inputStream))
            {
                if (image.Width > maxWidth || image.Height > maxHeight)
                {
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = new Size(maxWidth, maxHeight),
                        Mode = ResizeMode.Max
                    }));
                }

                await image.SaveAsync(outputStream, new JpegEncoder());
            }

            outputStream.Position = 0; // Reset stream position for further use
            return outputStream;
        }
    }
}
