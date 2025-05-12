namespace everave.server.Components.Common
{
    public interface IForeignAccess
    {
        bool IsEnabled { get; }
        bool IsBusy { get; }
        event Action IsBusyChanged;
    }
}
