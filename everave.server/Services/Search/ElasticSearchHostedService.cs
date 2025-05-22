using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Bulk;
using Elastic.Clients.Elasticsearch.QueryDsl;
using everave.server.Forum;
using MongoDB.Driver;

namespace everave.server.Services.Search
{
    public class ElasticSearchHostedService(IConfiguration configuration, IMongoDatabase database) : IHostedService
    {
        private readonly ElasticsearchClient _client = ElasticSearch.CreateClient(configuration);

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _client.Indices.CreateAsync(ElasticSearch.IndexName, cancellationToken);
            await SyncEntityAsync(ElasticSearch.IndexName, 
                database.GetCollection<Post>("posts"),
                database.GetCollection<Topic>("topics"));
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async Task SyncEntityAsync(string indexName, IMongoCollection<Post> postsCollection, IMongoCollection<Topic> topicsCollection)
        {
            // 1. Load everything from MongoDB
            var posts = await postsCollection.Find(_ => true).ToListAsync();
            var postsIdSet = posts.Select(d => d.Id.ToString()).ToHashSet();
            var topics = await topicsCollection.Find(_ => true).ToListAsync();
            var topicsIdSet = topics.Select(d => d.Id.ToString()).ToHashSet();

            var esIds = new HashSet<string>();

            // 2. Get all IDs from Elasticsearch using scroll
            var initialResponse = await _client.SearchAsync<SearchDocument>(s => s
                .Indices(ElasticSearch.IndexName)
                .Size(1000)
                .Scroll("2m")
                .Query(new MatchAllQuery())
            );

            if (!initialResponse.IsValidResponse)
                throw new Exception("Initial scroll search failed");

            foreach (var hit in initialResponse.Hits)
            {
                if (!string.IsNullOrEmpty(hit.Id))
                    esIds.Add(hit.Id);
            }

            var scrollId = initialResponse.ScrollId;

            // 2. Continue scrolling
            while (scrollId != null)
            {
                var scrollResponse = await _client.ScrollAsync<SearchDocument>(new ScrollRequest
                {
                    ScrollId = scrollId,
                    Scroll = "2m"
                });

                if (!scrollResponse.IsValidResponse || scrollResponse.Documents.Count == 0)
                    break;

                foreach (var hit in scrollResponse.Hits)
                {
                    if (!string.IsNullOrEmpty(hit.Id))
                        esIds.Add(hit.Id);
                }

                scrollId = scrollResponse.ScrollId;
            }

            // 3. Clear scroll context
            if (scrollId != null)
            {
                await _client.ClearScrollAsync(new ClearScrollRequest
                {
                    ScrollId = scrollId
                });
            }

            // 3. Delete stale documents
            var staleIds = esIds.Except(postsIdSet).Except(topicsIdSet).ToList();
            if (staleIds.Any())
            {
                var deleteOps = staleIds.Select(id => new BulkDeleteOperation<SearchDocument>(id)).ToList();
                var deleteRequest = new BulkRequest(indexName)
                {
                    Operations = new BulkOperationsCollection(deleteOps)
                };
                await _client.BulkAsync(deleteRequest);
            }

            // 4. Bulk index MongoDB docs
            var postIndices = posts.Select(post =>
                new BulkIndexOperation<SearchDocument>(SearchDocument.Create(post)) { Id = post.Id.ToString() }).ToList();

            var topicsIndices = topics.Select(topic =>
                new BulkIndexOperation<SearchDocument>(SearchDocument.Create(topic))).ToList();


            var indexRequest = new BulkRequest(indexName)
            {
                Operations = new BulkOperationsCollection(postIndices.Concat(topicsIndices))
            };

            if (postIndices.Count > 0)
            {
                var response = await _client.BulkAsync(indexRequest);
                if (!response.IsValidResponse)
                    throw new Exception($"Bulk indexing failed: {response.DebugInformation}");
            }
        }
    }
}
