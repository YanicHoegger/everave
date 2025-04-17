using MongoDB.Bson;
using MongoDB.Driver;

namespace everave.server.Forum
{
    public class ForumService : IForumService
    {
        private readonly IMongoCollection<ForumGroup> _forumGroups;
        private readonly IMongoCollection<Forum> _forums;
        private readonly IMongoCollection<Topic> _topics;
        private readonly IMongoCollection<Entry> _entries;

        private const int PageSize = 15;

        public ForumService(IMongoDatabase database)
        {
            _forumGroups = database.GetCollection<ForumGroup>("forumGroups");
            _forums = database.GetCollection<Forum>("forums");
            _topics = database.GetCollection<Topic>("topics");
            _entries = database.GetCollection<Entry>("entries");

            _topics.Indexes.CreateOne(new CreateIndexModel<Topic>(
            Builders<Topic>.IndexKeys.Ascending(t => t.ForumId)));

            _entries.Indexes.CreateOne(new CreateIndexModel<Entry>(
                Builders<Entry>.IndexKeys.Ascending(e => e.TopicId)));

            _forums.Indexes.CreateOne(new CreateIndexModel<Forum>(
                Builders<Forum>.IndexKeys.Ascending(f => f.GroupId)));
        }

        public async Task<Dictionary<ForumGroup, List<Forum>>> GetForumsGroupedAsync()
        {
            var groups = await _forumGroups.Find(_ => true).ToListAsync();
            var forums = await _forums.Find(_ => true).ToListAsync();

            var result = groups.ToDictionary(
                g => g,
                g => forums.Where(f => f.GroupId == g.Id).ToList()
            );

            return result;
        }

        public async Task<List<ForumGroup>> GetAllForumGroupsAsync() => 
            await _forumGroups.Find(_ => true ).ToListAsync();
       
        public async Task<List<Forum>> GetForumsByGroupIdAsync(ObjectId groupId) => 
            await _forums.Find(f => f.GroupId == groupId).ToListAsync();

        public async Task<Forum> GetForumAsync(ObjectId forum) =>
            await _forums.Find(f => f.Id == forum).FirstOrDefaultAsync();

        public async Task<List<Topic>> GetTopicsByForumIdAsync(ObjectId forumId) =>
            await _topics.Find(t => t.ForumId == forumId).ToListAsync();

        public async Task<List<Entry>> GetEntriesByThreadIdAsync(ObjectId threadId, int page = 1) =>
            await _entries
            .Find(e => e.TopicId == threadId)
            .Skip((page - 1) * PageSize)
            .Limit(PageSize)
            .SortBy(e => e.CreatedAt)
            .ToListAsync();

        public Task AddForumGroupAsync(ForumGroup group) =>
            _forumGroups.InsertOneAsync(group);

        public Task DeleteForumGroupAsync(ForumGroup group) => 
            _forumGroups.DeleteOneAsync(g => g.Id == group.Id);

        public Task AddForumAsync(Forum forum) =>
            _forums.InsertOneAsync(forum);

        public Task DeleteForumAsync(Forum forum) => 
            _forums.DeleteOneAsync(f => f.Id == forum.Id);

        public async Task AddTopicAsync(Topic topic)
        {
            await _topics.InsertOneAsync(topic);

            var update = Builders<Forum>.Update.Inc(f => f.NumberOfTopics, 1);
            await _forums.UpdateOneAsync(
                f => f.Id == topic.ForumId,
                update
            );
        }

        public async Task DeleteTopicAsync(Topic topic)
        {
            await _topics.DeleteOneAsync(t => t.Id == topic.Id);

            var update = Builders<Forum>.Update.Inc(f => f.NumberOfTopics, -1);
            await _forums.UpdateOneAsync(
                f => f.Id == topic.ForumId,
                update
            );
        }

        public async Task AddEntryAsync(Entry entry)
        {
            await _entries.InsertOneAsync(entry);

            var topicUpdate = Builders<Topic>.Update.Inc(t => t.NumberOfEntries, 1);
            await _topics.UpdateOneAsync(
                t => t.Id == entry.TopicId,
                topicUpdate
            );

            var topic = await _topics.Find(t => t.Id == entry.TopicId).FirstOrDefaultAsync();
            if (topic != null)
            {
                var forumUpdate = Builders<Forum>.Update.Inc(f => f.NumberOfEntries, 1);
                await _forums.UpdateOneAsync(
                    f => f.Id == topic.ForumId,
                    forumUpdate
                );
            }
        }

        public async Task DeleteEntryAsync(Entry entry)
        {
            await _entries.DeleteOneAsync(e => e.Id == entry.Id);

            var topicUpdate = Builders<Topic>.Update.Inc(t => t.NumberOfEntries, -1);
            await _topics.UpdateOneAsync(
                t => t.Id == entry.TopicId,
                topicUpdate
            );

            var topic = await _topics.Find(t => t.Id == entry.TopicId).FirstOrDefaultAsync();
            if (topic != null)
            {
                var forumUpdate = Builders<Forum>.Update.Inc(f => f.NumberOfEntries, -1);
                await _forums.UpdateOneAsync(
                    f => f.Id == topic.ForumId,
                    forumUpdate
                );
            }
        }
    }
}
