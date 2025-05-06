using everave.server.UserManagement;

namespace everave.server.Components.Common
{
    public static class ProfilePicture
    {
        public static string GetProfilePictureUrl(this ApplicationUser? user)
        {
            return string.IsNullOrEmpty(user?.ProfilePictureUrl)
                ? "/no_avatar.gif"
                : user.ProfilePictureUrl;
        }
    }
}
