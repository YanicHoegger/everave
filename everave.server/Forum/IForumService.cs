using MongoDB.Bson;

namespace everave.server.Forum
{
    public interface IForumService
    {
        Task<List<ForumGroup>> GetAllForumGroupsAsync();
        Task<List<Forum>> GetForumsByGroupIdAsync(ObjectId groupId);
        Task<Forum> GetForumAsync(ObjectId forum);
        Task<List<Topic>> GetTopicsByForumIdAsync(ObjectId forumId);
        Task<List<Entry>> GetEntriesByThreadIdAsync(ObjectId threadId, int page = 1);

        Task AddForumGroupAsync(ForumGroup group);
        Task AddForumAsync(Forum forum);
        Task AddTopicAsync(Topic topic);
        Task AddEntryAsync(Entry entry);
    }
}
