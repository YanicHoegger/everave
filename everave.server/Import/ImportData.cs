using everave.server.UserManagement;

namespace everave.server.Import
{
    public class ImportData
    {
        public required List<ForumGroup> ForumGroups { get; set; }
        public required List<User> Users { get; set; }

        public class User
        {
            public required string UserName { get; set; }
            public required string EncryptedPassword { get; set; }
            public string Signature { get; set; }
            public DateTime CreatedAt { get; set; }
            public string ProfilePictureUrl { get; set; }
            public UserDetails UserDetails { get; set; }
        }

        public class ForumGroup
        {
            public required string Description { get; set; }
            public required List<Forum> Forums { get; set; }
        }

        public class Forum
        {
            public required string Name { get; set; }
            public required string Description { get; set; }
            public required List<Topic> Topics { get; set; }
        }

        public class Topic
        {
            public required string Title { get; set; }
            public required string Author { get; set; }
            public required DateTime CreatedAt { get; set; }
            public required List<Post> Posts { get; set; }
        }

        public class Post
        {
            public required string Content { get; set; }
            public required string Author { get; set; }
            public required DateTime CreatedAt { get; set; }
        }
    }
}
