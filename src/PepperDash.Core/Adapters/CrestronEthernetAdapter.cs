using System;
using Crestron.SimplSharp;
using PepperDash.Core.Abstractions;

namespace PepperDash.Core.Adapters;

/// <summary>
/// Production adapter — delegates IEthernetHelper calls to the real Crestron SDK.
/// </summary>
public sealed class CrestronEthernetAdapter : IEthernetHelper
{
    public string GetEthernetParameter(EthernetParameterType parameter, short ethernetAdapterId)
    {
        var crestronParam = parameter switch
        {
            EthernetParameterType.GetCurrentIpAddress =>
                CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS,
            EthernetParameterType.GetHostname =>
                CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_HOSTNAME,
            EthernetParameterType.GetDomainName =>
                CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_DOMAIN_NAME,
            EthernetParameterType.GetLinkStatus =>
                CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_LINK_STATUS,
            EthernetParameterType.GetCurrentDhcpState =>
                CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_DHCP_STATE,
            EthernetParameterType.GetCurrentIpMask =>
                CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_MASK,
            EthernetParameterType.GetCurrentRouter =>
                CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_ROUTER,
            EthernetParameterType.GetMacAddress =>
                CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_MAC_ADDRESS,
            EthernetParameterType.GetDnsServer =>
                CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_DNS_SERVER,
            _ => throw new ArgumentOutOfRangeException(nameof(parameter), parameter, null),
        };

        return CrestronEthernetHelper.GetEthernetParameter(crestronParam, ethernetAdapterId);
    }
}
