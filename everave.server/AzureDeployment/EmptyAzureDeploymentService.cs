namespace everave.server.AzureDeployment
{
    public class EmptyAzureDeploymentService : IAzureDeploymentService
    {
        public bool IsEnabled => false;
        public Task<List<Slot>> GetSlots()
        {
            throw new NotSupportedException();
        }
    }
}
