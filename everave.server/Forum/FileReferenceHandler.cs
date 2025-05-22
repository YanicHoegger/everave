using everave.server.Services;
using System.Text.RegularExpressions;

namespace everave.server.Forum
{
    public class FileReferenceHandler(IImageStorageService imageStorageService)
    {
        private static readonly Regex ImageRegex = new("<img[^>]*src=[\"']([^\"']+)[\"'][^>]*>", RegexOptions.IgnoreCase);

        public async Task DeleteFileReferences(Post post)
        {
            var content = post.HtmlContent;

            foreach (var extractImageFilename in ExtractImageFilenames(content))
            {
                await imageStorageService.DeleteImageAsync(Path.GetFileName(extractImageFilename));
            }
        }

        private static List<string> ExtractImageFilenames(string htmlContent)
        {
            var filenames = new List<string>();

            var matches = ImageRegex.Matches(htmlContent);

            foreach (Match match in matches)
            {
                var src = match.Groups[1].Value;

                // Get only the filename from the path
                var filename = Path.GetFileName(src);
                if (!string.IsNullOrEmpty(filename))
                {
                    filenames.Add(filename);
                }
            }

            return filenames;
        }
    }
}
