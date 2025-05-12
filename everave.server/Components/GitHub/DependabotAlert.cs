namespace everave.server.Components.GitHub
{
    public class DependabotAlert
    {
        public string PackageName { get; set; }
        public string Severity { get; set; }
        public string CurrentVersion { get; set; }
        public string FixedVersion { get; set; }
        public string AdvisoryUrl { get; set; }
    }
}
