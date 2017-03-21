using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Core.Ethernet
{
	public static class EthernetSettings
	{
		public static readonly BoolFeedback LinkActive = new BoolFeedback(EthernetCue.LinkActive,
			() => true);
		public static readonly BoolFeedback DhcpActive = new BoolFeedback(EthernetCue.DhcpActive,
			() => CrestronEthernetHelper.GetEthernetParameter(
				CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_DHCP_STATE, 0) == "ON");


		public static readonly StringFeedback Hostname = new StringFeedback(EthernetCue.Hostname,
			() => CrestronEthernetHelper.GetEthernetParameter(
				CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_HOSTNAME, 0));
		public static readonly StringFeedback IpAddress0 = new StringFeedback(EthernetCue.IpAddress0,
			() => CrestronEthernetHelper.GetEthernetParameter(
				CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, 0));
		public static readonly StringFeedback SubnetMask0 = new StringFeedback(EthernetCue.SubnetMask0,
			() => CrestronEthernetHelper.GetEthernetParameter(
				CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_MASK, 0));
		public static readonly StringFeedback DefaultGateway0 = new StringFeedback(EthernetCue.DefaultGateway0,
			() => CrestronEthernetHelper.GetEthernetParameter(
				CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_ROUTER, 0));	
	}

	public static class EthernetCue
	{
		public static readonly Cue LinkActive = Cue.BoolCue("LinkActive", 1);
		public static readonly Cue DhcpActive = Cue.BoolCue("DhcpActive", 2);

		public static readonly Cue Hostname = Cue.StringCue("Hostname", 1);
		public static readonly Cue IpAddress0 = Cue.StringCue("IpAddress0", 2);
		public static readonly Cue SubnetMask0 = Cue.StringCue("SubnetMask0", 3);
		public static readonly Cue DefaultGateway0 = Cue.StringCue("DefaultGateway0", 4);
	}
}