using everave.server.UserManagement;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Driver;

namespace everave.server.Forum
{
    public class ForumService : IForumService
    {
        private readonly FileReferenceHandler _fileReferenceHandler;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMongoCollection<ForumGroup> _forumGroups;
        private readonly IMongoCollection<Forum> _forums;
        private readonly IMongoCollection<Topic> _topics;
        private readonly IMongoCollection<Post> _posts;

        public ForumService(IMongoDatabase database, FileReferenceHandler fileReferenceHandler, UserManager<ApplicationUser> userManager)
        {
            _fileReferenceHandler = fileReferenceHandler;
            _userManager = userManager;
            _forumGroups = database.GetCollection<ForumGroup>("forumGroups");
            _forums = database.GetCollection<Forum>("forums");
            _topics = database.GetCollection<Topic>("topics");
            _posts = database.GetCollection<Post>("posts");

            _topics.Indexes.CreateOne(new CreateIndexModel<Topic>(
            Builders<Topic>.IndexKeys.Ascending(t => t.ForumId)));

            _posts.Indexes.CreateOne(new CreateIndexModel<Post>(
                Builders<Post>.IndexKeys.Ascending(e => e.TopicId)));

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

        public Task<List<Post>> GetPostsByTopicIdAsync(ObjectId topicId, int page = 1) =>
            _posts
                .Find(e => e.TopicId == topicId)
                .Skip((page - 1) * PageSize)
                .Limit(PageSize)
                .SortBy(e => e.CreatedAt)
                .ToListAsync();

        public Task<List<Post>> GetPostsByUserIdAsync(ObjectId userId, int page = 1)
        {
            return _posts
                .Find(e => e.UserId == userId)
                .Skip((page - 1) * PageSize)
                .Limit(PageSize)
                .SortByDescending(e => e.CreatedAt)
                .ToListAsync();
        }

        public Task<Post> GetPostById(ObjectId entryId) =>
            _posts.Find(e => e.Id == entryId).FirstAsync();

        public async Task<int> GetPageOfPostAsync(ObjectId entryId)
        {
            var entry = await _posts.Find(e => e.Id == entryId).FirstOrDefaultAsync();
            if (entry == null)
            {
                throw new ArgumentException("Post not found", nameof(entryId));
            }

            var entriesBeforeCount = await _posts
                .Find(e => e.TopicId == entry.TopicId && e.CreatedAt < entry.CreatedAt)
                .CountDocumentsAsync();

            return (int)entriesBeforeCount / PageSize + 1;
        }


        public Task AddForumGroupAsync(ForumGroup group) =>
            _forumGroups.InsertOneAsync(group);

        public async Task DeleteForumGroupAsync(ForumGroup group)
        {
            var forums = await _forums.Find(f => f.GroupId == group.Id).ToListAsync();
            foreach (var forum in forums)
            {
                await DeleteForumAsync(forum);
            }

            await _forumGroups.DeleteOneAsync(g => g.Id == group.Id);
        }

        public Task AddForumAsync(Forum forum) =>
            _forums.InsertOneAsync(forum);

        public async Task DeleteForumAsync(Forum forum)
        {
            var topics = await _topics.Find(e => e.ForumId == forum.Id).ToListAsync();
            foreach (var topic in topics)
            {
                await DeleteTopicAsync(topic);
            }

            await _forums.DeleteOneAsync(f => f.Id == forum.Id);
        }

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
            var posts = await _posts
                .Find(e => e.TopicId == topic.Id).ToListAsync();

            foreach (var post in posts)
            {
                await DeletePostAsync(post);
            }

            await _topics.DeleteOneAsync(t => t.Id == topic.Id);

            var update = Builders<Forum>.Update.Inc(f => f.NumberOfTopics, -1);
            await _forums.UpdateOneAsync(
                f => f.Id == topic.ForumId,
                update
            );
        }

        public async Task AddPostAsync(Post post)
        {
            await _posts.InsertOneAsync(post);

            var topicUpdate = Builders<Topic>.Update
                .Inc(t => t.NumberOfEntries, 1)
                .Set(t => t.LastEntry, post.Id);

            await _topics.UpdateOneAsync(
                t => t.Id == post.TopicId,
                topicUpdate
            );

            var topic = await _topics.Find(t => t.Id == post.TopicId).FirstOrDefaultAsync();
            if (topic != null)
            {
                var forumUpdate = Builders<Forum>.Update
                    .Inc(f => f.NumberOfEntries, 1)
                    .Set(f => f.LastEntry, post.Id);
                await _forums.UpdateOneAsync(
                    f => f.Id == topic.ForumId,
                    forumUpdate
                );
            }

            var user = await _userManager.FindByIdAsync(post.UserId.ToString());
            if (user != null)
            {
                user.NumberOfPosts++;
                await _userManager.UpdateAsync(user);
            }
        }

        public async Task DeletePostAsync(Post post)
        {
            await _fileReferenceHandler.DeleteFileReferences(post);

            await _posts.DeleteOneAsync(e => e.Id == post.Id);

            var topicUpdate = Builders<Topic>.Update.Inc(t => t.NumberOfEntries, -1);
            await _topics.UpdateOneAsync(
                t => t.Id == post.TopicId,
                topicUpdate
            );

            var topic = await _topics.Find(t => t.Id == post.TopicId).FirstOrDefaultAsync();
            if (topic != null)
            {
                if (topic.LastEntry == post.Id)
                {
                    var oldestEntry = await _posts
                        .Find(e => e.TopicId == post.TopicId)
                        .SortBy(e => e.CreatedAt)
                        .FirstOrDefaultAsync();

                    var lastEntryUpdate = Builders<Topic>.Update.Set(t => t.LastEntry, oldestEntry?.Id);
                    await _topics.UpdateOneAsync(
                        t => t.Id == post.TopicId,
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
                    if (forum.LastEntry == post.Id)
                    {
                        var oldestEntryInForum = await _posts
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

            var user = await _userManager.FindByIdAsync(post.UserId.ToString());
            if (user != null)
            {
                user.NumberOfPosts--;
                await _userManager.UpdateAsync(user);
            }
        }
    }
}
