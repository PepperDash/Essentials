﻿using System;
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
	/// <summary>
	/// Represents the configuration data for a single tie line between two routing ports.
	/// </summary>
	public class TieLineConfig
	{
		/// <summary>
		/// The key of the source device.
		/// </summary>
		public string SourceKey { get; set; }
		
		/// <summary>
		/// The key of the source card (if applicable, e.g., in a modular chassis).
		/// </summary>
		public string SourceCard { get; set; }
		
		/// <summary>
		/// The key of the source output port.
		/// </summary>
		public string SourcePort { get; set; }
		
		/// <summary>
		/// The key of the destination device.
		/// </summary>
		public string DestinationKey { get; set; }
		
		/// <summary>
		/// The key of the destination card (if applicable).
		/// </summary>
		public string DestinationCard { get; set; }
		
		/// <summary>
		/// The key of the destination input port.
		/// </summary>
		public string DestinationPort { get; set; }

        /// <summary>
        /// Optional override for the signal type of the tie line. If set, this overrides the destination port's type for routing calculations.
        /// </summary>
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

		/// <summary>
		/// Logs an error message related to creating this tie line configuration.
		/// </summary>
		/// <param name="msg">The specific error message.</param>
		void LogError(string msg)
		{
			Debug.LogMessage(LogEventLevel.Error, "WARNING: Cannot create tie line: {message}:\r   {tieLineConfig}",null, msg, this);
		}

		/// <summary>
		/// Returns a string representation of the tie line configuration.
		/// </summary>
		/// <returns>A string describing the source and destination of the configured tie line.</returns>
		public override string ToString()
		{
			return string.Format("{0}.{1}.{2} --> {3}.{4}.{5}", SourceKey, SourceCard, SourcePort,
				DestinationKey, DestinationCard, DestinationPort);
		}
	}
}