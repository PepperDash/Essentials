using System.Linq;
using Newtonsoft.Json;

using PepperDash.Essentials.Core;
using PepperDash.Core;

namespace PepperDash.Essentials
{
	public class ConfigTieLine
	{
		[JsonProperty(Required=Required.Always)]
		public string SourceDeviceKey { get; set; }

		[JsonProperty(Required = Required.Always)]
		public string SourcePortKey { get; set; }

		[JsonProperty(Required = Required.Always)]
		public string DestinationDeviceKey { get; set; }

		[JsonProperty(Required = Required.Always)]
		public string DestinationPortKey { get; set; }

		public override string ToString()
		{
			return string.Format("Tie line: [{0}]{1} --> [{2}]{3}", SourceDeviceKey, SourcePortKey, DestinationDeviceKey, DestinationPortKey);
		}

		/// <summary>
		/// Returns a tie line if one can be constructed between the two devices and ports
		/// </summary>
		/// <returns>TieLine or null if devices or ports don't exist</returns>
		public TieLine GetTieLine()
		{
			var sourceDevice = (IRoutingOutputs)DeviceManager.GetDeviceForKey(SourceDeviceKey);
			var destinationDevice = (IRoutingInputs)DeviceManager.GetDeviceForKey(DestinationDeviceKey);

			if (sourceDevice == null)
			{
				Debug.Console(0, "    Cannot create TieLine. Source device '{0}' not found or does not have outputs",
					SourceDeviceKey);
				return null;
			}
			else if (destinationDevice == null)
			{
				Debug.Console(0, "    Cannot create TieLine. Destination device '{0}' not found or does not have inputs",
					DestinationDeviceKey);
				return null;
			}
			else
			{
				// Get the ports by key name from the lists
				RoutingOutputPort sourcePort = sourceDevice.OutputPorts.FirstOrDefault(
				    p => p.Key.Equals(SourcePortKey, System.StringComparison.OrdinalIgnoreCase));
				//RoutingOutputPort sourcePort = null;
				//sourceDevice.OutputPorts.TryGetValue(SourcePortKey, out sourcePort);
				if (sourcePort == null)
				{
					Debug.Console(0, "    Cannot create TieLine {0}-->{1}. Source device does not have output port '{2}'",
						sourceDevice.Key, destinationDevice.Key, SourcePortKey);
					return null;
				}

				RoutingInputPort destinationPort = destinationDevice.InputPorts.FirstOrDefault(
					p => p.Key.Equals(DestinationPortKey, System.StringComparison.OrdinalIgnoreCase));
				//RoutingInputPort destinationPort = null;
				//destinationDevice.InputPorts.TryGetValue(DestinationPortKey, out destinationPort);
				if (destinationPort == null)
				{
					Debug.Console(0, "    Cannot create TieLine {0}-->{1}. Destination device does not have input port '{2}'",
						sourceDevice.Key, destinationDevice.Key, DestinationPortKey);
					return null;
				}

				var tl = new TieLine(sourcePort, destinationPort);
				Debug.Console(1, "    Created {0}", this);
				return tl;
			}
		}

	}

}