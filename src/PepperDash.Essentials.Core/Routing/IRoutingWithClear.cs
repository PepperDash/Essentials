namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Defines a routing device (<see cref="IRouting"/>) that supports explicitly clearing a route on an output.
    /// </summary>
    public interface IRoutingWithClear : IRouting
    {
        /// <summary>
        /// Clears a route to an output, however a device needs to do that
        /// </summary>
        /// <param name="outputSelector">Output to clear</param>
        /// <param name="signalType">signal type to clear</param>
        void ClearRoute(object outputSelector, eRoutingSignalType signalType);
    }
}