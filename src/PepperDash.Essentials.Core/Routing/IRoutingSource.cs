namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Marker interface to identify a device that acts as the origin of a signal path (<see cref="IRoutingOutputs"/>).
    /// </summary>
    public interface IRoutingSource : IRoutingOutputs
    {
    }
}