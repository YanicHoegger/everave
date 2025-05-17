using everave.server.Services;
using System.Text.RegularExpressions;

namespace everave.server.Forum
{
    public class FileReferenceHandler(IImageStorageService imageStorageService)
    {
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

            var regex = new Regex("<img[^>]*src=[\"']([^\"']+)[\"'][^>]*>", RegexOptions.IgnoreCase);
            var matches = regex.Matches(htmlContent);

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
