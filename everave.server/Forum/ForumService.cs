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

        public int PageSize => 15;

        public Task<List<ForumGroup>> GetAllForumGroupsAsync() =>
            _forumGroups.Find(_ => true).ToListAsync();

        public Task<List<Forum>> GetForumsByGroupIdAsync(ObjectId groupId) =>
            _forums.Find(f => f.GroupId == groupId).ToListAsync();

        public Task<Forum> GetForumAsync(ObjectId forum) =>
            _forums.Find(f => f.Id == forum).FirstOrDefaultAsync();

        public Task<List<Topic>> GetTopicsByForumIdAsync(ObjectId forumId) =>
            _topics.Find(t => t.ForumId == forumId).ToListAsync();

        public Task<List<Topic>> GetTopicsByForumIdAsync(ObjectId forumId, int page) =>
            _topics
                .Find(t => t.ForumId == forumId)
                .Skip((page - 1) * PageSize)
                .Limit(PageSize)
                .SortByDescending(t => t.CreatedAt)
                .ToListAsync();

        public Task<Topic> GetTopicByIdAsync(ObjectId topicId) =>
            _topics.Find(t => t.Id == topicId).FirstAsync();

        public Task<List<Entry>> GetEntriesByTopicIdAsync(ObjectId topicId, int page = 1) =>
            _entries
                .Find(e => e.TopicId == topicId)
                .Skip((page - 1) * PageSize)
                .Limit(PageSize)
                .SortBy(e => e.CreatedAt)
                .ToListAsync();

        public Task<Entry> GetEntryById(ObjectId entryId) =>
            _entries.Find(e => e.Id == entryId).FirstAsync();

        public async Task<int> GetPageOfEntryAsync(ObjectId entryId)
        {
            var entry = await _entries.Find(e => e.Id == entryId).FirstOrDefaultAsync();
            if (entry == null)
            {
                throw new ArgumentException("Entry not found", nameof(entryId));
            }

            var entriesBeforeCount = await _entries
                .Find(e => e.TopicId == entry.TopicId && e.CreatedAt < entry.CreatedAt)
                .CountDocumentsAsync();

            return (int)entriesBeforeCount / PageSize + 1;
        }


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

            var topicUpdate = Builders<Topic>.Update
                .Inc(t => t.NumberOfEntries, 1)
                .Set(t => t.LastEntry, entry.Id);
            await _topics.UpdateOneAsync(
                t => t.Id == entry.TopicId,
                topicUpdate
            );

            var topic = await _topics.Find(t => t.Id == entry.TopicId).FirstOrDefaultAsync();
            if (topic != null)
            {
                var forumUpdate = Builders<Forum>.Update
                    .Inc(f => f.NumberOfEntries, 1)
                    .Set(f => f.LastEntry, entry.Id);
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
                if (topic.LastEntry == entry.Id)
                {
                    var oldestEntry = await _entries
                        .Find(e => e.TopicId == entry.TopicId)
                        .SortBy(e => e.CreatedAt)
                        .FirstOrDefaultAsync();

                    var lastEntryUpdate = Builders<Topic>.Update.Set(t => t.LastEntry, oldestEntry?.Id);
                    await _topics.UpdateOneAsync(
                        t => t.Id == entry.TopicId,
                        lastEntryUpdate
                    );
                }

                var forumUpdate = Builders<Forum>.Update
                    .Inc(f => f.NumberOfEntries, -1);
                await _forums.UpdateOneAsync(
                    f => f.Id == topic.ForumId,
                    forumUpdate
                );

                var forum = await _forums.Find(f => f.Id == topic.ForumId).FirstOrDefaultAsync();
                if (forum != null)
                {
                    if (forum.LastEntry == entry.Id)
                    {
                        var oldestEntryInForum = await _entries
                            .Find(e => e.TopicId == topic.Id)
                            .SortBy(e => e.CreatedAt)
                            .FirstOrDefaultAsync();

                        var forumLastEntryUpdate = Builders<Forum>.Update.Set(f => f.LastEntry, oldestEntryInForum?.Id);
                        await _forums.UpdateOneAsync(
                            f => f.Id == topic.ForumId,
                            forumLastEntryUpdate
                        );
                    }
                }
            }
        }
    }
}
