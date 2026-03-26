using Newtonsoft.Json;
using PepperDash.Core;


namespace PepperDash.Essentials.Core;

/// <summary>
/// Defines the contract for IRoutingOutputs
/// </summary>
public interface IRoutingOutputs : IKeyed
{
    /// <summary>
    /// Collection of Output Ports
    /// </summary>
    [JsonProperty("outputPorts")]
    RoutingPortCollection<RoutingOutputPort> OutputPorts { get; }
}

