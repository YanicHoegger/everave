namespace everave.server.Services.Search
{
    public class EmptySearch : ISearchService
    {
        public Task<List<SearchDocument>> SearchPostsAsync(string query)
        {
            return Task.FromResult(new List<SearchDocument>());
        }
    }
}
