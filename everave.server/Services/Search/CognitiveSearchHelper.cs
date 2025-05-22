using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;

namespace everave.server.Services.Search
{
    public static class CognitiveSearchHelper
    {
        public static SearchIndexClient GetSearchIndexClient(IConfiguration configuration)
        {
            var endpoint = new Uri(configuration["CognitiveSearchUrl"]);
            var azureKeyCredential = new AzureKeyCredential(configuration["CongitiveSearchKey"]);
            return new SearchIndexClient(endpoint, azureKeyCredential);
        }

        public static SearchClient GetSearchClient(IConfiguration configuration)
        {
            var endpoint = new Uri(configuration["CognitiveSearchUrl"]);
            var azureKeyCredential = new AzureKeyCredential(configuration["CongitiveSearchKey"]);
            return new SearchClient(endpoint, CognitiveSearchService.IndexName, azureKeyCredential);
        }
    }
}
