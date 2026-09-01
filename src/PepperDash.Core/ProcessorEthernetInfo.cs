using System;
using Crestron.SimplSharp;

namespace PepperDash.Core;

/// <summary>
/// Helpers for reading the processor's own network addresses.
/// </summary>
/// <remarks>
/// <c>CrestronEthernetHelper.GetEthernetParameter</c> does not throw when it is asked for a parameter
/// belonging to an adapter that the processor does not have — it returns the literal string
/// <c>"Invalid Value"</c>. Callers that only null/empty-check the result end up building URLs such as
/// <c>wss://Invalid Value:65479/debug/join</c>. These helpers return <c>null</c> in that case so callers
/// can fall back cleanly.
/// </remarks>
public static class ProcessorEthernetInfo
{
    /// <summary>
    /// The sentinel string the Crestron SDK returns for a parameter that cannot be read.
    /// </summary>
    public const string InvalidValue = "Invalid Value";

    /// <summary>
    /// Returns the supplied network parameter, or <c>null</c> when it is blank or the SDK's
    /// <c>"Invalid Value"</c> sentinel.
    /// </summary>
    /// <param name="value">The raw value returned by <c>GetEthernetParameter</c>.</param>
    /// <returns>The trimmed value, or <c>null</c> when it is not usable.</returns>
    public static string NullIfInvalid(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();

        return trimmed.Equals(InvalidValue, StringComparison.OrdinalIgnoreCase) ? null : trimmed;
    }

    /// <summary>
    /// Gets the current IP address of the processor's LAN adapter, or <c>null</c> when it cannot be read.
    /// </summary>
    /// <remarks>
    /// Falls back to adapter id 0 on platforms (such as Virtual Control) where the adapter type lookup
    /// is not supported.
    /// </remarks>
    public static string GetLanIpAddress() =>
        GetIpAddressForAdapterType(EthernetAdapterType.EthernetLANAdapter) ?? GetIpAddressForAdapterId(0);

    /// <summary>
    /// Gets the current IP address of the processor's control subnet (CS LAN) adapter, or <c>null</c>
    /// when the processor has no control subnet.
    /// </summary>
    public static string GetCsLanIpAddress() =>
        GetIpAddressForAdapterType(EthernetAdapterType.EthernetCSAdapter);

    /// <summary>
    /// Gets the current IP address for the specified adapter type, or <c>null</c> when that adapter is
    /// not present on this processor.
    /// </summary>
    /// <param name="adapterType">The adapter type to look up.</param>
    public static string GetIpAddressForAdapterType(EthernetAdapterType adapterType)
    {
        try
        {
            var adapterId = CrestronEthernetHelper.GetAdapterdIdForSpecifiedAdapterType(adapterType);

            return adapterId < 0 ? null : GetIpAddressForAdapterId(adapterId);
        }
        catch (ArgumentException)
        {
            // This processor does not have an adapter of the requested type.
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the current IP address for the specified adapter id, or <c>null</c> when it cannot be read.
    /// </summary>
    /// <param name="adapterId">The Crestron ethernet adapter id.</param>
    public static string GetIpAddressForAdapterId(short adapterId)
    {
        try
        {
            return NullIfInvalid(CrestronEthernetHelper.GetEthernetParameter(
                CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, adapterId));
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the specified parameter for the given adapter id, or <c>null</c> when it cannot be read.
    /// </summary>
    /// <param name="parameter">The parameter to read.</param>
    /// <param name="adapterId">The Crestron ethernet adapter id.</param>
    public static string GetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET parameter, short adapterId)
    {
        try
        {
            return NullIfInvalid(CrestronEthernetHelper.GetEthernetParameter(parameter, adapterId));
        }
        catch (Exception)
        {
            return null;
        }
    }
}
