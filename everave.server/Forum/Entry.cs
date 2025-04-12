using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace everave.server.Forum
{
    public class Entry
    {
        public ObjectId Id { get; set; }
        [BsonRepresentation(BsonType.String)]
        public required Guid UserId { get; set; }
        public required string HtmlContent { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
