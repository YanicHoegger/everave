using everave.server.Components.Common;

namespace everave.server.Components.GitHub
{
    public interface IGitHubAccess : IForeignAccess
    {
        Task<List<DependabotPR>> GetDependabotPRsAsync();
        Task ApprovePr(DependabotPR dependabotPr);
        Task<List<DependabotAlert>> GetDependabotAlertsAsync();
    }
}
