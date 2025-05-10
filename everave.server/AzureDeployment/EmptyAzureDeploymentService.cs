namespace everave.server.AzureDeployment
{
    public class EmptyAzureDeploymentService : IAzureDeploymentService
    {
        public bool IsEnabled => false;
        public Task<List<Slot>> GetSlots()
        {
            throw new NotSupportedException();
        }

        public Task TransferToProductionAsync(Slot slot)
        {
            throw new NotSupportedException();
        }

        public Task DeleteSlot(Slot slot)
        {
            throw new NotSupportedException();
        }
    }
}
