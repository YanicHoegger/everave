using Azure.Search.Documents.Indexes;

namespace everave.server.Services.Search
{
    public class CognitiveServiceDocument
    {
        [SimpleField(IsKey = true)]
        public string Id { get; set; }
        public string TopicId { get; set; }

        [SearchableField(AnalyzerName = "de.lucene")]
        public string? Content { get; set; }

        [SearchableField(AnalyzerName = "de.lucene")]
        public string? Topic { get; set; }

        [SimpleField(IsFilterable = true)]
        public string Category { get; set; }

        [SimpleField(IsSortable = true)]
        public DateTime CreatedAt { get; set; }

        public static SearchType GetSearchType(string type)
        {
            return Enum.Parse<SearchType>(type);
        }

        public static string GetSearchTypeString(SearchType type)
        {
            return Enum.GetName(typeof(SearchType), type) ?? string.Empty;
        }

        public static SearchDocument CreateSearchDocument(CognitiveServiceDocument cognitiveServiceDocument)
        {
            return new SearchDocument
            {
                Id = cognitiveServiceDocument.Id,
                TopicId = cognitiveServiceDocument.TopicId,
                Topic = cognitiveServiceDocument.Topic,
                Content = cognitiveServiceDocument.Content,
                Type = GetSearchType(cognitiveServiceDocument.Category),
                CreatedAt = cognitiveServiceDocument.CreatedAt
            };
        }

        public static CognitiveServiceDocument CreateSearchDocument(SearchDocument searchDocument)
        {
            return new CognitiveServiceDocument
            {
                Id = searchDocument.Id,
                TopicId = searchDocument.TopicId,
                Topic = searchDocument.Topic,
                Content = searchDocument.Content,
                Category = GetSearchTypeString(searchDocument.Type),
                CreatedAt = searchDocument.CreatedAt
            };
        }
    }
}
