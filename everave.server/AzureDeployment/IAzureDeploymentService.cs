namespace everave.server.AzureDeployment
{
    public interface IAzureDeploymentService
    {
        bool IsEnabled { get; }

        Task<List<Slot>> GetSlots();
        Task TransferToProductionAsync(Slot slot);
        Task DeleteSlot(Slot slot);
    }
}
