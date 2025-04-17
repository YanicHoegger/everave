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
        Task DeleteForumGroupAsync(ForumGroup group);
        Task AddForumAsync(Forum forum);
        Task DeleteForumAsync(Forum forum);
        Task AddTopicAsync(Topic topic);
        Task DeleteTopicAsync(Topic topic);
        Task AddEntryAsync(Entry entry);
        Task DeleteEntryAsync(Entry entry);
    }
}
