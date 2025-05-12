namespace everave.server.Components.GitHub
{
    public class NoGitHubAccess : IGitHubAccess
    {
        public bool IsEnabled => false;
        public bool IsBusy => false;
        public event Action? IsBusyChanged { add { } remove { } }

        public Task<List<DependabotPR>> GetDependabotPRsAsync()
        {
            throw new NotSupportedException();
        }

        public Task ApprovePr(DependabotPR dependabotPr)
        {
            throw new NotSupportedException();
        }

        public Task<List<DependabotAlert>> GetDependabotAlertsAsync()
        {
            throw new NotSupportedException();
        }
    }
}
