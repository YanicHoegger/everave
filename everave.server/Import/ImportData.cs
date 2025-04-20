namespace everave.server.Import
{
    public class ImportData
    {
        public List<ForumGroup> ForumGroups { get; set; }
        public List<User> Users { get; set; }

        public class User
        {
            public string Username { get; set; }
            public string EncryptedPassword { get; set; }
        }

        public class ForumGroup
        {
            public string Description { get; set; }
            public List<Forum> Forums { get; set; }
        }

        public class Forum
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public List<Topic> Topics { get; set; }
        }

        public class Topic
        {
            public string Title { get; set; }
            public User Author { get; set; }
            public DateTime CreatedAt { get; set; }
            public List<Entry> Entries { get; set; }
        }

        public class Entry
        {
            public string Content { get; set; } = default!;
            public User Author { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}
