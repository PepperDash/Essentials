

using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharpPro;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Core.Config
{
	public class TieLineConfig
	{
		public string SourceKey { get; set; }
		public string SourceCard { get; set; }
		public string SourcePort { get; set; }
		public string DestinationKey { get; set; }
		public string DestinationCard { get; set; }
		public string DestinationPort { get; set; }

		/// <summary>
		/// Returns the appropriate tie line for either a card-based device or 
		/// regular device with ports on-device.
		/// </summary>
		/// <returns>null if config data does not match ports, cards or devices</returns>
		public TieLine GetTieLine()
		{
			Debug.Console(0, "Build TieLine: {0}", this);
			// Get the source device
			var sourceDev = DeviceManager.GetDeviceForKey(SourceKey) as IRoutingOutputs;
			if (sourceDev == null)
			{
				LogError("Routable source not found");
				return null;
			}

			// Get the destination device
			var destDev = DeviceManager.GetDeviceForKey(DestinationKey) as IRoutingInputs;
			if (destDev == null)
			{
				LogError("Routable destination not found");
				return null;
			}

            //Get the source port
            RoutingOutputPort sourceOutputPort = null;
            //// If it's a card-based device, get the card and then the source port
            //if (sourceDev is ICardPortsDevice)
            //{
            //    if (SourceCard == null)
            //    {
            //        LogError("Card missing from source device config");
            //        return null;
            //    }
            //    sourceOutputPort = (sourceDev as ICardPortsDevice).GetChildOutputPort(SourceCard, SourcePort);
            //    if (sourceOutputPort == null)
            //    {
            //        LogError("Source card does not contain port");
            //        return null;
            //    }
            //}
            //// otherwise it's a normal port device, get the source port
            //else
            //{
				sourceOutputPort = sourceDev.OutputPorts[SourcePort];
				if (sourceOutputPort == null)
				{
					LogError("Source does not contain port");
					return null;
				}
            //}


			//Get the Destination port
			RoutingInputPort destinationInputPort = null;
            //// If it's a card-based device, get the card and then the Destination port
            //if (destDev is ICardPortsDevice)
            //{
            //    if (DestinationCard == null)
            //    {
            //        LogError("Card missing from destination device config");
            //        return null;
            //    }
            //    destinationInputPort = (destDev as ICardPortsDevice).GetChildInputPort(DestinationCard, DestinationPort);
            //    if (destinationInputPort == null)
            //    {
            //        LogError("Destination card does not contain port");
            //        return null;
            //    }
            //}
            //// otherwise it's a normal port device, get the Destination port
            //else
            //{
				destinationInputPort = destDev.InputPorts[DestinationPort];
				if (destinationInputPort == null)
				{
					LogError("Destination does not contain port");
					return null;
				}
            //}

			return new TieLine(sourceOutputPort, destinationInputPort);
		}

		void LogError(string msg)
		{
			Debug.Console(1, "WARNING: Cannot create tie line: {0}:\r   {1}", msg, this);
		}

		public override string ToString()
		{
			return string.Format("{0}.{1}.{2} --> {3}.{4}.{5}", SourceKey, SourceCard, SourcePort,
				DestinationKey, DestinationCard, DestinationPort);
		}
	}
}