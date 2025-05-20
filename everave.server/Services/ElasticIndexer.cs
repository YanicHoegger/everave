using Elastic.Clients.Elasticsearch;
using everave.server.Forum;

namespace everave.server.Services
{
    public class ElasticIndexer(IForumNotifier forumNotifier, IConfiguration configuration) : IHostedService
    {
        private readonly ElasticsearchClient _client = ElasticSearch.CreateClient(configuration);

        public Task StartAsync(CancellationToken cancellationToken)
        {
            forumNotifier.PostAdded += IndexPost;
            forumNotifier.PostDeleted += DeletePost;
            forumNotifier.TopicAdded += IndexTopic;
            forumNotifier.TopicDeleted += DeleteTopic;

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            forumNotifier.PostAdded -= IndexPost;
            forumNotifier.PostDeleted -= DeletePost;
            forumNotifier.TopicAdded -= IndexTopic;
            forumNotifier.TopicDeleted -= DeleteTopic;

            return Task.CompletedTask;
        }

        public async Task IndexPost(Post post)
        {
            var doc = SearchDocument.Create(post);
            await _client.IndexAsync(doc, ElasticSearch.IndexName, doc.Id);
        }

        private async Task IndexTopic(Topic topic)
        {
            var doc = SearchDocument.Create(topic);
            await _client.IndexAsync(doc, ElasticSearch.IndexName, doc.Id);
        }

        private async Task DeleteTopic(Topic topic)
        {
            await _client.DeleteAsync<SearchDocument>(topic.Id.ToString());
        }

        public async Task DeletePost(Post post)
        {
            await _client.DeleteAsync<SearchDocument>(post.Id.ToString());
        }
    }
}
