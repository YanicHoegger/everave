namespace everave.server.Services
{
    public interface ISearchService
    {
        Task<List<SearchDocument>> SearchPostsAsync(string query);
    }
}
