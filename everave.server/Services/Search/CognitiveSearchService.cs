using Azure;
using Azure.Search.Documents;

namespace everave.server.Services.Search
{
    public class CognitiveSearchService(IConfiguration configuration) : ISearchService
    {
        public const string IndexName = "posts-index";
        private readonly SearchClient _searchClient = new(
            new Uri(configuration["CognitiveSearchUrl"]),
            IndexName,
            new AzureKeyCredential(configuration["CongitiveSearchKey"]));


        public async Task<List<SearchDocument>> SearchPostsAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return [];

            var options = new SearchOptions
            {
                Size = 10,
                IncludeTotalCount = true
            };

            options.Select.Add(nameof(CognitiveServiceDocument.Id));
            options.Select.Add(nameof(CognitiveServiceDocument.Topic));
            options.Select.Add(nameof(CognitiveServiceDocument.TopicId));
            options.Select.Add(nameof(CognitiveServiceDocument.Content));
            options.Select.Add(nameof(CognitiveServiceDocument.Category));
            options.Select.Add(nameof(CognitiveServiceDocument.CreatedAt));

            var response = await _searchClient.SearchAsync<CognitiveServiceDocument>(query, options);

            return response.Value.GetResults()
                .Select(x => CognitiveServiceDocument.CreateSearchDocument(x.Document))
                .ToList();
        }
    }
}
