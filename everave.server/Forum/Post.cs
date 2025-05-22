using MongoDB.Bson;

namespace everave.server.Forum
{
    public class Post
    {
        public ObjectId Id { get; set; }
        public ObjectId TopicId { get; set; }
        public ObjectId UserId { get; set; }
        public required string HtmlContent { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
