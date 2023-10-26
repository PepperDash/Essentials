namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Represents a class that has a basic communication monitoring
    /// </summary>
    public interface ICommunicationMonitor
    {
        StatusMonitorBase CommunicationMonitor { get; }
    }
}