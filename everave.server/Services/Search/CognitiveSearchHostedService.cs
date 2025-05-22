using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using everave.server.Forum;
using MongoDB.Driver;

namespace everave.server.Services.Search
{
    public class CognitiveSearchHostedService(IConfiguration configuration, IMongoDatabase database) : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var adminClient = CognitiveSearchHelper.GetSearchIndexClient(configuration);
            var searchClient = CognitiveSearchHelper.GetSearchClient(configuration);

            await CreateIndex(cancellationToken, adminClient);
            await SyncDataBase(cancellationToken, searchClient, database.GetCollection<Post>("posts"),
                database.GetCollection<Topic>("topics"));
        }

        private static async Task CreateIndex(CancellationToken cancellationToken, SearchIndexClient adminClient)
        {
            var fieldBuilder = new FieldBuilder();
            var searchFields = fieldBuilder.Build(typeof(CognitiveServiceDocument));

            var definition = new SearchIndex(CognitiveSearchService.IndexName, searchFields);

            await adminClient.CreateOrUpdateIndexAsync(definition, cancellationToken: cancellationToken);
        }

        private static async Task SyncDataBase(
            CancellationToken cancellationToken,
            SearchClient searchClient,
            IMongoCollection<Post> postsCollection,
            IMongoCollection<Topic> topicsCollection)
        {
            // 1. Load everything from MongoDB
            var posts = await postsCollection.Find(_ => true).ToListAsync(cancellationToken);
            var postsIdSet = posts.Select(d => d.Id.ToString()).ToHashSet();

            var topics = await topicsCollection.Find(_ => true).ToListAsync(cancellationToken);
            var topicsIdSet = topics.Select(d => d.Id.ToString()).ToHashSet();

            var allSourceIds = postsIdSet.Union(topicsIdSet).ToHashSet();

            // 2. Fetch all documents currently in the Azure Search index
            var indexIds = new HashSet<string>();
            var options = new SearchOptions
            {
                Size = 1000,
                Select = { "Id" }
            };

            await foreach (var result in (await searchClient.SearchAsync<CognitiveServiceDocument>("*", options, cancellationToken)).Value.GetResultsAsync())
            {
                indexIds.Add(result.Document.Id);
            }

            // 3. Delete stale documents
            var staleIds = indexIds.Except(allSourceIds).ToList();
            if (staleIds.Any())
            {
                var documentsToDelete = staleIds.Select(id => new SearchDocument { Id = id }).ToList();
                await searchClient.DeleteDocumentsAsync(documentsToDelete, cancellationToken: cancellationToken);
            }

            // 4. Create and upload new/updated documents
            var allDocs = new List<SearchDocument>();

            allDocs.AddRange(posts.Select(SearchDocument.Create));
            allDocs.AddRange(topics.Select(SearchDocument.Create));

            if (allDocs.Count > 0)
            {
                await searchClient.MergeOrUploadDocumentsAsync(allDocs.Select(CognitiveServiceDocument.CreateSearchDocument), cancellationToken: cancellationToken);
            }
        }


        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
