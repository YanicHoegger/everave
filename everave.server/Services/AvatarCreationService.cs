using everave.server.Components.Controllers;
using Microsoft.AspNetCore.Identity;
using everave.server.UserManagement;
using Slugify;

namespace everave.server.Services
{
    public class AvatarCreationService(
        IImageStorageService imageStorageService,
        UserManager<ApplicationUser> userManager)
    {
        private static readonly SlugHelper SlugHelper = new();

        public async Task<string> Create(Stream imageStream, ApplicationUser user, string fileName)
        {
            fileName = Create(fileName, user.UserName);

            await using var resizedStream = await ImageProcessingHelper
                .ResizeImageAsync(imageStream, 200, 200).ConfigureAwait(false);

            var success = await imageStorageService.UploadImageAsync(resizedStream, fileName);

            if (success)
            {
                if (!string.IsNullOrWhiteSpace(user.ProfilePictureUrl))
                    await imageStorageService.DeleteImageAsync(Path.GetFileName(user.ProfilePictureUrl));

                user.ProfilePictureUrl = $"/image/{fileName}";

                await userManager.UpdateAsync(user);
            }

            return fileName;
        }

        private static string Create(string fileName, string userName)
        {
            var normalizedUserName = SlugHelper.GenerateSlug(userName);
            return $"Avatar_{normalizedUserName}_{Guid.NewGuid()}{Path.GetExtension(fileName)}";
        }
    }
}
