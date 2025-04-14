using MongoDB.Bson;

namespace everave.server.Forum
{
    public class ForumGroup
    {
        public ObjectId Id { get; set; }
        public string Description { get; set; } = default!;
    }
}
