using PepperDash.Core;
using PepperDash.Essentials.Core.Routing;

namespace PepperDash.Essentials.Core;

/// <summary>
/// Defines the contract for IRoutingSink
/// </summary>
public interface IRoutingSink : IRoutingInputs, IKeyName, ICurrentSources
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


