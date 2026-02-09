using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Core
{
	/// <summary>
	/// IAttachVideoStatusExtensions class
	/// </summary>
	public static class IAttachVideoStatusExtensions
	{
		/// <summary>
		/// Gets the VideoStatusOutputs for the device
		/// </summary>
		/// <param name="attachedDev"></param>
		/// <returns>Attached VideoStatusOutputs or the default if none attached</returns>
		public static VideoStatusOutputs GetVideoStatuses(this IAttachVideoStatus attachedDev)
		{
			// See if this device is connected to a status-providing port
			var tl = TieLineCollection.Default.FirstOrDefault(t =>
				t.SourcePort.ParentDevice == attachedDev
				&& t.DestinationPort is RoutingInputPortWithVideoStatuses);
			if (tl != null)
			{
				// if so, and it's got status, return it -- or null
				var port = tl.DestinationPort as RoutingInputPortWithVideoStatuses;
				if (port != null)
					return port.VideoStatus;
			}
			return VideoStatusOutputs.NoStatus;
		}

  /// <summary>
  /// HasVideoStatuses method
  /// </summary>
		public static bool HasVideoStatuses(this IAttachVideoStatus attachedDev)
		{
			return TieLineCollection.Default.FirstOrDefault(t =>
				t.SourcePort.ParentDevice == attachedDev
				&& t.DestinationPort is RoutingInputPortWithVideoStatuses) != null;
		}
	}
}