using everave.server.Forum;

namespace everave.server.Services
{
    public class SearchDocument
    {
        public string Id { get; set; }
        public string TopicId { get; set; }
        public DateTime CreatedAt { get; set; }
        public SearchType Type { get; set; }
        public string? Topic { get; set; }
        public string? Content { get; set; }

        public static SearchDocument Create(Post post)
        {
            return new SearchDocument
            {
                Content = post.HtmlContent,
                CreatedAt = post.CreatedAt,
                Id = post.Id.ToString(),
                TopicId = post.TopicId.ToString(),
                Type = SearchType.Post
            };
        }
        public static SearchDocument Create(Topic topic)
        {
            return new SearchDocument
            {
                Topic = topic.Title,
                CreatedAt = topic.CreatedAt,
                Id = topic.Id.ToString(),
                TopicId = topic.Id.ToString(),
                Type = SearchType.Topic
            };
        }
    }

    public enum SearchType
    {
        Topic,
        Post
    }
}