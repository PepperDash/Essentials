using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Core.Ethernet
{
	/// <summary>
	/// Ethernet settings feedbacks
	/// </summary>
	public static class EthernetSettings
	{
		/// <summary>
		/// Link active feedback
		/// </summary>
		public static readonly BoolFeedback LinkActive = new BoolFeedback("LinkActive",
			() => true);

		/// <summary>
		/// DHCP active feedback
		/// </summary>	
		public static readonly BoolFeedback DhcpActive = new BoolFeedback("DhcpActive",
			() => CrestronEthernetHelper.GetEthernetParameter(
				CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_DHCP_STATE, 0) == "ON");

		/// <summary>
		/// Hostname feedback
		/// </summary>
		public static readonly StringFeedback Hostname = new StringFeedback("Hostname",
			() => CrestronEthernetHelper.GetEthernetParameter(
				CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_HOSTNAME, 0));

		/// <summary>
		/// IP Address feedback
		/// </summary>
		public static readonly StringFeedback IpAddress0 = new StringFeedback("IpAddress0",
			() => CrestronEthernetHelper.GetEthernetParameter(
				CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, 0));

		/// <summary>
		/// Subnet Mask feedback
		/// </summary>
		public static readonly StringFeedback SubnetMask0 = new StringFeedback("SubnetMask0",
			() => CrestronEthernetHelper.GetEthernetParameter(
				CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_MASK, 0));

		/// <summary>
		/// Default Gateway feedback
		/// </summary>
		public static readonly StringFeedback DefaultGateway0 = new StringFeedback("DefaultGateway0",
			() => CrestronEthernetHelper.GetEthernetParameter(
				CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_ROUTER, 0));	
	}
}