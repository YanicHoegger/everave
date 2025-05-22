using Azure.Search.Documents;
using everave.server.Forum;

namespace everave.server.Services.Search
{
    public class CognitiveSearchIndexer(IConfiguration configuration, IForumNotifier forumNotifier) : IHostedService
    {
        private readonly SearchClient _searchClient = CognitiveSearchHelper.GetSearchClient(configuration);

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
            await _searchClient.UploadDocumentsAsync([
                CognitiveServiceDocument.CreateSearchDocument(SearchDocument.Create(post))
            ]);
        }

        private async Task IndexTopic(Topic topic)
        {
            await _searchClient.UploadDocumentsAsync([
                CognitiveServiceDocument.CreateSearchDocument(SearchDocument.Create(topic))
            ]);
        }

        private async Task DeleteTopic(Topic topic)
        {
            await _searchClient.DeleteDocumentsAsync([new CognitiveServiceDocument { Id = topic.Id.ToString() }]);
        }

        public async Task DeletePost(Post post)
        {
            await _searchClient.DeleteDocumentsAsync([new CognitiveServiceDocument { Id = post.Id.ToString() }]);
        }
    }
}
