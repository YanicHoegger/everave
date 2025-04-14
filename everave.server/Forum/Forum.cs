using MongoDB.Bson;

namespace everave.server.Forum
{
    public class Forum
    {
        public ObjectId Id { get; set; }
        public ObjectId GroupId { get; set; }
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public int NumberOfTopics { get; set; }
        public int NumberOfEntries { get; set; }
    }
}
