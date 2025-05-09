namespace everave.server.AzureDeployment
{
    public class Slot(string name, string url, bool isRunning)
    {
        public string Name { get; } = name;
        public string Url { get; } = url;
        public bool IsRunning { get; } = isRunning;
    }
}
