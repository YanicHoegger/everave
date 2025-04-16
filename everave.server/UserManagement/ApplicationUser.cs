using AspNetCore.Identity.Mongo.Model;

namespace everave.server.UserManagement
{
    public class ApplicationUser : MongoUser
    {
        public const string AdminRole = "Admin";
        public string DisplayName { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
    }
}
