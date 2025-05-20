namespace everave.server.Services
{
    public class EmptySearch : ISearchService
    {
        public Task<List<SearchDocument>> SearchPostsAsync(string query)
        {
            return Task.FromResult(new List<SearchDocument>());
        }
    }
}
