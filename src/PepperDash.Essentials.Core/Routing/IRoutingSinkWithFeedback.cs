using PepperDash.Core;
using PepperDash.Essentials.Core.Routing;

namespace PepperDash.Essentials.Core;

/// <summary>
/// Delegate for InputChangedEventHandler.
/// </summary>
/// <param name="destination">The sink device that changed input.</param>
/// <param name="currentPort">The new input port selected on the sink device.</param>
public delegate void InputChangedEventHandler(IRoutingSinkWithFeedback destination, RoutingInputPort currentPort);

/// <summary>
/// Defines a routing sink (endpoint) device that can switch inputs and provides feedback.
/// Consolidates the former IRoutingSink, IRoutingSinkWithInputPort, IRoutingSinkWithSwitching,
/// IRoutingSinkWithSwitchingWithInputPort, and IRoutingSinkWithFeedback interfaces.
/// </summary>
public interface IRoutingSinkWithFeedback : IRoutingInputs, IKeyName, ICurrentSources
{
    /// <summary>
    /// Executes a switch on the device.
    /// </summary>
    /// <param name="inputSelector">Input selector.</param>
    void ExecuteSwitch(object inputSelector);

    /// <summary>
    /// Gets the current input port for this routing sink.
    /// </summary>
    RoutingInputPort CurrentInputPort { get; }

    /// <summary>
    /// Event raised when the input changes.
    /// </summary>
    event InputChangedEventHandler InputChanged;
}



