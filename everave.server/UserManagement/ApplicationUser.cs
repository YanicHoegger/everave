using AspNetCore.Identity.Mongo.Model;

namespace everave.server.UserManagement
{
    public class ApplicationUser : MongoUser
    {
        public const string AdminRole = "Admin";
        public string? ProfilePictureUrl { get; set; }
        public int NumberOfPosts { get; set; }
        public string Signature { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public UserDetails UserDetails { get; set; }
    }

    public record UserDetails
    {
        public Gender Gender { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Location { get; set; }
        public string? Interests { get; set; }
        public string? Occupation { get; set; }
    }

    public enum Gender
    {
        Male = 0,
        Female = 1,
        Other = 3,
        Unspecified
    }
}
