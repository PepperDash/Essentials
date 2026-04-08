namespace PepperDash.Core.Abstractions;

/// <summary>
/// Abstracts <c>Crestron.SimplSharp.CrestronEthernetHelper</c> to allow unit testing
/// without the Crestron SDK.
/// </summary>
public interface IEthernetHelper
{
    /// <summary>
    /// Returns a network parameter string for the specified adapter.
    /// </summary>
    /// <param name="parameter">The parameter to retrieve.</param>
    /// <param name="ethernetAdapterId">Ethernet adapter index (0 = LAN A).</param>
    /// <returns>String value of the requested parameter.</returns>
    string GetEthernetParameter(EthernetParameterType parameter, short ethernetAdapterId);
}
