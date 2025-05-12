namespace everave.server.Components.AzureDeployment
{
    public class EmptyAzureDeploymentService : IAzureDeploymentService
    {
        public bool IsEnabled => false;
        public bool IsBusy => false;
        public event Action? IsBusyChanged { add { } remove { } }

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
