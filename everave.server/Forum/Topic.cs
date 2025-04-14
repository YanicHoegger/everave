using MongoDB.Bson;

namespace everave.server.Forum
{
    public class Topic
    {
        public ObjectId Id { get; set; }
        public ObjectId ForumId { get; set; }
        public string Title { get; set; } = default!;
        public string CreatedByUserId { get; set; } = default!;
        public string CreatedByUsername { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public int NumberOfEntries { get; set; }
    }
}
