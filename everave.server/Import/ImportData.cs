namespace everave.server.Import
{
    public class ImportData
    {
        public required List<ForumGroup> ForumGroups { get; set; }
        public required List<User> Users { get; set; }

        public class User
        {
            public required string Username { get; set; }
            public required string EncryptedPassword { get; set; }
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
            public required List<Entry> Entries { get; set; }
        }

        public class Entry
        {
            public required string Content { get; set; }
            public required string Author { get; set; }
            public required DateTime CreatedAt { get; set; }
        }
    }
}
