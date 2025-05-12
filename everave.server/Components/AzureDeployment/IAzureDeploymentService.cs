using everave.server.Components.Common;

namespace everave.server.Components.AzureDeployment
{
    public interface IAzureDeploymentService : IForeignAccess
    {
        Task<List<Slot>> GetSlots();
        Task TransferToProductionAsync(Slot slot);
        Task DeleteSlot(Slot slot);
    }
}
