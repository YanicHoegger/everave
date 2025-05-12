using Azure;
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

        var webApp = await GetProductionResource();

        await foreach (var slot in webApp.GetWebSiteSlots().GetAllAsync())
        {
            var item = new Slot(
                slot.Data.Name,
                slot.Data.HostNames.FirstOrDefault() ?? "No URL",
                slot.Data.Id );

            var temp = slot.Data.SystemData?.CreatedOn;

            slots.Add(item);
        }

        return slots;
    }


    public async Task TransferToProductionAsync(Slot slot)
    {
        var slotResource = await GetSlotResource(slot);
        var productionWebApp = await GetProductionResource();

        var productionConfig = (await productionWebApp.GetWebSiteConfig().GetAsync()).Value;
        var slotConfig = (await slotResource.GetWebSiteSlotConfig().GetAsync()).Value;

        productionConfig.Data.LinuxFxVersion = slotConfig.Data.LinuxFxVersion;

        await productionConfig.CreateOrUpdateAsync(WaitUntil.Completed, productionConfig.Data);

        await DeleteSlot(slotResource);
    }

    public async Task DeleteSlot(Slot slot)
    {
        var webApp = await GetSlotResource(slot);

        await DeleteSlot(webApp);
    }

    private static async Task DeleteSlot(WebSiteSlotResource webApp)
    {
        await webApp.DeleteAsync(WaitUntil.Completed, true);
    }

    private async Task<WebSiteSlotResource> GetSlotResource(Slot slot)
    {
        var webApp = _armClient.GetWebSiteSlotResource(new ResourceIdentifier(slot.ResourceId));

        var webAppResponse = await webApp.GetAsync();
        if (webAppResponse == null)
        {
            throw new InvalidOperationException($"Web app '{_configuration[AzureSiteNameKey]}/{slot.Name}' does not exist or is not accessible.");
        }

        return webApp;
    }

    private async Task<WebSiteResource> GetProductionResource()
    {
        var webAppResourceId = WebSiteResource.CreateResourceIdentifier(_configuration[AzureSubscriptionIdKey], _configuration[AzureResourceGroupKey], _configuration[AzureSiteNameKey]);
        var webApp = _armClient.GetWebSiteResource(new ResourceIdentifier(webAppResourceId));

        var webAppResponse = await webApp.GetAsync();
        if (webAppResponse == null)
        {
            throw new InvalidOperationException($"Web app '{_configuration[AzureSiteNameKey]}' does not exist or is not accessible.");
        }

        return webApp;
    }
}
