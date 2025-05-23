﻿using MongoDB.Bson;

namespace everave.server.Forum
{
    public interface IForumService
    {
        int PageSize { get; }
        Task<List<ForumGroup>> GetAllForumGroupsAsync();
        Task<List<Forum>> GetForumsByGroupIdAsync(ObjectId groupId);
        Task<Forum> GetForumAsync(ObjectId forum);
        Task<List<Topic>> GetTopicsByForumIdAsync(ObjectId forumId);
        Task<List<Topic>> GetTopicsByForumIdAsync(ObjectId forumId, int page);
        Task<Topic> GetTopicByIdAsync(ObjectId topicId);
        Task<List<Post>> GetPostsByTopicIdAsync(ObjectId topicId, int page = 1);
        Task<List<Post>> GetPostsByUserIdAsync(ObjectId userId, int page = 1);
        Task<Post> GetPostById(ObjectId entryId);

        Task AddForumGroupAsync(ForumGroup group);
        Task DeleteForumGroupAsync(ForumGroup group);
        Task AddForumAsync(Forum forum);
        Task DeleteForumAsync(Forum forum);
        Task AddTopicAsync(Topic topic);
        Task DeleteTopicAsync(Topic topic);
        Task AddPostAsync(Post post);
        Task DeletePostAsync(Post post);
        Task<int> GetPageOfPostAsync(ObjectId entryId);
    }
}
