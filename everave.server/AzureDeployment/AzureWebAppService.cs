using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.AppService;

namespace everave.server.AzureDeployment;

public class AzureWebAppService : IAzureDeploymentService
{
    private readonly IConfiguration _configuration;
    private readonly ArmClient _armClient;

    private const string AzureTenantIdKey = "AZURE_TENANT_ID";
    private const string AzureClientIdKey = "AZURE_CLIENT_ID";
    private const string AzureClientSecretKey = "AZURE_CLIENT_SECRET";
    private const string AzureSubscriptionIdKey = "AZURE_SUBSCRIPTION_ID";
    private const string AzureResourceGroupKey = "AZURE_RESOURCE_GROUP";
    private const string AzureSiteNameKey = "AZURE_SITE_NAME";

    public AzureWebAppService(IConfiguration configuration)
    {
        _configuration = configuration;
        var clientSecretCredential = new ClientSecretCredential(
            configuration[AzureTenantIdKey],
            configuration[AzureClientIdKey],
            configuration[AzureClientSecretKey]);

        _armClient = new ArmClient(clientSecretCredential);
    }

    public bool IsEnabled => AreRequiredConfigurationsSet();

    private bool AreRequiredConfigurationsSet()
    {
        var requiredKeys = new[]
        {
            AzureTenantIdKey,
            AzureClientIdKey,
            AzureClientSecretKey,
            AzureSubscriptionIdKey,
            AzureResourceGroupKey,
            AzureSiteNameKey
        };

        return requiredKeys.All(key => !string.IsNullOrWhiteSpace(_configuration[key]));
    }

    public async Task<List<Slot>> GetSlots()
    {
        var slots = new List<Slot>();

        var webAppResourceId = $"/subscriptions/{_configuration[AzureSubscriptionIdKey]}/resourceGroups/{_configuration[AzureResourceGroupKey]}/providers/Microsoft.Web/sites/{_configuration[AzureSiteNameKey]}";
        var webApp = _armClient.GetWebSiteResource(new ResourceIdentifier(webAppResourceId));

        var webAppResponse = await webApp.GetAsync();
        if (webAppResponse == null)
        {
            throw new InvalidOperationException($"Web app '{_configuration[AzureSiteNameKey]}' does not exist or is not accessible.");
        }

        await foreach (var slot in webApp.GetWebSiteSlots().GetAllAsync())
        {
            var url = slot.Data.HostNames.FirstOrDefault() ?? "No URL";
            var item = new Slot(
                slot.Data.Name,
                url,
                await IsSlotReachableAsync(url));

            slots.Add(item);
        }

        return slots;
    }

    //TODO: Use that in different task
    public async Task<bool> IsSlotReachableAsync(string slotUrl)
    {
        if (string.IsNullOrWhiteSpace(slotUrl) || slotUrl == "No URL")
        {
            return false;
        }

        using var httpClient = new HttpClient();
        try
        {
            var response = await httpClient.GetAsync($"https://{slotUrl}");
            return response.IsSuccessStatusCode; 
        }
        catch
        {
            return false;
        }
    }
}
