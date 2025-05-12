using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace everave.server.Components.GitHub
{
    public class GitHubAccess : IGitHubAccess
    {
        private readonly HttpClient _http;
        private bool _isBusy;

        public GitHubAccess(IConfiguration config)
        {
            if (string.IsNullOrWhiteSpace(config["GitHub:Owner"]) 
                || string.IsNullOrWhiteSpace(config["GitHub:Repo"])
                || string.IsNullOrWhiteSpace(config["GitHub:Token"]))
            {
                IsEnabled = false;
                return;
            }

            IsEnabled = true;
            _http = new HttpClient
            {
                BaseAddress = new Uri($"https://api.github.com/repos/{config["GitHub:Owner"]}/{config["GitHub:Repo"]}/")
            };
            _http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("App", "1.0"));
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("token", config["GitHub:Token"]);
        }

        public async Task<List<DependabotPR>> GetDependabotPRsAsync()
        {
            IsBusy = true;
            var prs = await _http.GetFromJsonAsync<JsonElement[]>("pulls");
            IsBusy = false;
            return prs?
                .Where(pr => pr.GetProperty("user").GetProperty("login").GetString() == "dependabot[bot]")
                .Select(pr => new DependabotPR
                {
                    Number = pr.GetProperty("number").GetInt32(),
                    Title = pr.GetProperty("title").GetString(),
                    Url = pr.GetProperty("html_url").GetString(),
                    CreatedAt = pr.GetProperty("created_at").GetString(),
                    Status = pr.GetProperty("state").GetString()
                }).ToList();
        }

        public async Task ApprovePr(DependabotPR dependabotPr)
        {
            IsBusy = true;
            var payload = new
            {
                commit_title = $"Merge PR {dependabotPr.Title}",
                merge_method = "squash" // or "merge" or "rebase"
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var mergeUrl = $"pulls/{dependabotPr.Number}/merge";
            var mergeResponse = await _http.PutAsync(mergeUrl, content);

            if (!mergeResponse.IsSuccessStatusCode)
            {
                var error = await mergeResponse.Content.ReadAsStringAsync();
                throw new Exception($"Failed to merge PR: {mergeResponse.StatusCode} - {error}");
            }
            IsBusy = false;
        }

        public async Task<List<DependabotAlert>> GetDependabotAlertsAsync()
        {
            IsBusy = true;

            var response = await _http.GetFromJsonAsync<JsonElement[]>($"dependabot/alerts");

            IsBusy = false;

            return response?.Select(a => new DependabotAlert
            {
                PackageName = a.GetProperty("dependency").GetProperty("package").GetProperty("name").GetString(),
                Severity = a.GetProperty("security_advisory").GetProperty("severity").GetString(),
                CurrentVersion = a.GetProperty("dependency").GetProperty("version").GetString(),
                FixedVersion = a.GetProperty("security_advisory").GetProperty("vulnerabilities")[0]
                    .GetProperty("first_patched_version").GetProperty("identifier").GetString(),
                AdvisoryUrl = a.GetProperty("html_url").GetString()
            }).ToList() ?? [];
        }

        public bool IsEnabled { get; }

        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                _isBusy = value;
                IsBusyChanged?.Invoke();
            }
        }

        public event Action? IsBusyChanged;
    }
}
