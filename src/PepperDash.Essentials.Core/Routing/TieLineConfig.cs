

using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharpPro;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using Serilog.Events;

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

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public eRoutingSignalType? OverrideType { get; set; }

		/// <summary>
		/// Returns the appropriate tie line for either a card-based device or 
		/// regular device with ports on-device.
		/// </summary>
		/// <returns>null if config data does not match ports, cards or devices</returns>
		public TieLine GetTieLine()
		{
			Debug.LogMessage(LogEventLevel.Information, "Build TieLine: {0}",null, this);
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
            var sourceOutputPort = sourceDev.OutputPorts[SourcePort];

            if (sourceOutputPort == null)
			{
				LogError("Source does not contain port");
				return null;
			}

            //Get the Destination port
            var destinationInputPort = destDev.InputPorts[DestinationPort];

            if (destinationInputPort == null)
				{
					LogError("Destination does not contain port");
					return null;
				}            

			return new TieLine(sourceOutputPort, destinationInputPort, OverrideType);
		}

		void LogError(string msg)
		{
			Debug.LogMessage(LogEventLevel.Error, "WARNING: Cannot create tie line: {message}:\r   {tieLineConfig}",null, msg, this);
		}

		public override string ToString()
		{
			return string.Format("{0}.{1}.{2} --> {3}.{4}.{5}", SourceKey, SourceCard, SourcePort,
				DestinationKey, DestinationCard, DestinationPort);
		}
	}
}