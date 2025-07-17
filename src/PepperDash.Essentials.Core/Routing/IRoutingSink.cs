using PepperDash.Essentials.Core.Routing;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// For fixed-source endpoint devices
    /// </summary>
    public interface IRoutingSink : IRoutingInputs, IHasCurrentSourceInfoChange
    {
    }

    /// <summary>
    /// For fixed-source endpoint devices with an input port
    /// </summary>
    public interface IRoutingSinkWithInputPort : IRoutingSink
    {
        /// <summary>
        /// Gets the current input port for this routing sink.
        /// </summary>
        RoutingInputPort CurrentInputPort { get; }
    }

    /// <summary>
    /// Interface for routing sinks that have access to the current source information.
    /// </summary>
    public interface IRoutingSinkWithCurrentSources : IRoutingSink, ICurrentSources
    {
    }
}