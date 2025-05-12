namespace everave.server.Components.GitHub
{
    public interface IGitHubAccess
    {
        Task<List<DependabotPR>> GetDependabotPRsAsync();
        Task ApprovePr(DependabotPR dependabotPr);
        Task<List<DependabotAlert>> GetDependabotAlertsAsync();
    }
}
