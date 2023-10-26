namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Simplified routing direct from source to destination
    /// </summary>
    public interface IRunDirectRouteAction
    {
        void RunDirectRoute(string sourceKey, string destinationKey);
    }
}