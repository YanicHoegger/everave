namespace everave.server.AzureDeployment
{
    public class Slot(string name, string url)
    {
        public string Name { get; } = name;
        public string Url { get; } = url;

        public async Task<bool> IsSlotReachableAsync()
        {
            if (string.IsNullOrWhiteSpace(Url) || Url == "No URL")
            {
                return false;
            }

            using var httpClient = new HttpClient();
            try
            {
                var response = await httpClient.GetAsync($"https://{Url}");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
