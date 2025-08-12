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
		/// The key of the source output port, used for routing configurations.
		/// </summary>
		public string SourcePort { get; set; }

		/// <summary>
		/// Gets or sets the DestinationKey
		/// </summary>
		public string DestinationKey { get; set; }

		/// <summary>
		/// Gets or sets the DestinationCard
		/// </summary>
		public string DestinationCard { get; set; }

		/// <summary>
		/// Gets or sets the DestinationPort
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
			Debug.LogInformation("Build TieLine: {config}", ToString());

			// Get the source device
			if (!(DeviceManager.GetDeviceForKey(SourceKey) is IRoutingOutputs sourceDev))
			{
				LogError("Routable source not found");
				return null;
			}

			// Get the destination device
			if (!(DeviceManager.GetDeviceForKey(DestinationKey) is IRoutingInputs destDev))
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
		private void LogError(string msg)
		{
			Debug.LogError("Cannot create tie line: {message}", msg);
		}

		/// <summary>
		/// Returns a string representation of the tie line configuration.
		/// </summary>
		/// <returns>A string describing the source and destination of the configured tie line.</returns>
		public override string ToString()
		{
			return $"{SourceKey}.{SourceCard}.{SourcePort} --> {DestinationKey}.{DestinationCard}.{DestinationPort}";
		}
	}
}