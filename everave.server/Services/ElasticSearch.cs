using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;

namespace everave.server.Services
{
    public class ElasticSearch(IConfiguration configuration) : ISearchService
    {
        public const string IndexName = "posts";
        private readonly ElasticsearchClient _client = CreateClient(configuration);

        public static ElasticsearchClient CreateClient(IConfiguration configuration)
        {
            return new ElasticsearchClient(new Uri(configuration["ElasticSearchUrl"]));
        }

        public async Task<List<SearchDocument>> SearchPostsAsync(string query)
        {
            var response = await _client.SearchAsync<SearchDocument>(s => s
                .Indices(IndexName)
                .Query(q => q
                    .Bool(b => b
                        .Should(
                            sh => sh.MultiMatch(mm => mm
                                .Fields(x => x.Topic)
                                .Boost(2)
                                .Query(query)
                                .Fuzziness("AUTO")
                                .Type(TextQueryType.MostFields)
                            ),
                            sh => sh.MultiMatch(mm => mm
                                .Fields(x => x.Content)
                                .Query(query)
                                .Fuzziness("AUTO")
                                .Type(TextQueryType.MostFields)
                            )
                        )
                        .MinimumShouldMatch(1)
                    )
                )
                .Size(15)
            );

            if (!response.IsValidResponse)
                throw new Exception("Search failed: " + response.ElasticsearchServerError?.Error.Reason);

            return response.Documents.ToList();
        }
    }
}
