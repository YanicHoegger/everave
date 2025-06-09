using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace everave.server.Components.GitHub
{
    public class GitHubAccess : IGitHubAccess
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _http;
        private bool _isBusy;

        public GitHubAccess(IConfiguration config)
        {
            _config = config;

            if (string.IsNullOrWhiteSpace(config["GitHub:Owner"])
                || string.IsNullOrWhiteSpace(config["GitHub:Repo"])
                || string.IsNullOrWhiteSpace(config["GitHub:AppId"])
                || string.IsNullOrWhiteSpace(config["GitHub:PrivateKey"])
                || string.IsNullOrWhiteSpace(config["GitHub:InstallationId"]))
            {
                IsEnabled = false;
                return;
            }

            _http = new HttpClient
            {
                BaseAddress = new Uri($"https://api.github.com/repos/{config["GitHub:Owner"]}/{config["GitHub:Repo"]}/")
            };
            _http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("App", "1.0"));

            IsEnabled = true;
        }

        private async Task EnsureAuthenticatedAsync()
        {
            var base64 = _config["GitHub:PrivateKey"];
            var pem = Encoding.UTF8.GetString(Convert.FromBase64String(base64));
            var jwt = GitHubJwtGenerator.GenerateJwt(
                _config["GitHub:AppId"],
                pem
            );

            var installationId = _config["GitHub:InstallationId"];

            using var authClient = new HttpClient();
            authClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
            authClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("App", "1.0"));
            authClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

            var response = await authClient.PostAsync(
                $"https://api.github.com/app/installations/{installationId}/access_tokens", null);

            var json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new Exception($"GitHub token error: {response.StatusCode} - {json}");

            var doc = JsonDocument.Parse(json);
            var token = doc.RootElement.GetProperty("token").GetString();

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }


        public async Task<List<DependabotPR>> GetDependabotPRsAsync()
        {
            IsBusy = true;
            await EnsureAuthenticatedAsync();

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
            await EnsureAuthenticatedAsync();

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
            await EnsureAuthenticatedAsync();

            var response = await _http.GetFromJsonAsync<JsonElement[]>("dependabot/alerts");

            IsBusy = false;

            return response?.Select(a =>
            {
                var dependabotAlert = new DependabotAlert
                {
                    PackageName = a.GetProperty("dependency").GetProperty("package").GetProperty("name").GetString(),
                    Severity = a.GetProperty("security_advisory").GetProperty("severity").GetString(),
                    AdvisoryUrl = a.GetProperty("html_url").GetString(),
                    State = a.GetProperty("state").GetString(),
                    Description = a.GetProperty("security_advisory").GetProperty("description").GetString(),
                    Summery = a.GetProperty("security_advisory").GetProperty("summary").GetString()
                };

                return dependabotAlert;
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
