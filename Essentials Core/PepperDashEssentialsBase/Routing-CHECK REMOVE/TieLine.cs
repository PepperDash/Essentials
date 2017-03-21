using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;

using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
	public class TieLine
	{
		public RoutingOutputPort SourcePort { get; private set; }
		public RoutingInputPort DestinationPort { get; private set; }
		public int InUseCount { get { return DestinationUsingThis.Count; } }

		/// <summary>
		/// Gets the type of this tie line.  Will either be the type of the desination port
		/// or the type of OverrideType when it is set.
		/// </summary>
		public eRoutingSignalType Type
		{
			get
			{
				if (OverrideType.HasValue) return OverrideType.Value;
				return DestinationPort.Type;
			}
		}

		/// <summary>
		/// Use this to override the Type property for the destination port. For example,
		/// when the tie line is type AudioVideo, and the signal flow should be limited to
		/// Audio-only or Video only, changing this type will alter the signal paths
		/// available to the routing algorithm without affecting the actual Type
		/// of the destination port.
		/// </summary>
		public eRoutingSignalType? OverrideType { get; set; }

		List<IRoutingInputs> DestinationUsingThis = new List<IRoutingInputs>();

		/// <summary>
		/// For tie lines that represent internal links, like from cards to the matrix in a DM.
		/// This property is true if SourcePort and DestinationPort IsInternal
		/// property are both true
		/// </summary>
		public bool IsInternal { get { return SourcePort.IsInternal && DestinationPort.IsInternal; } }
		public bool TypeMismatch { get { return SourcePort.Type != DestinationPort.Type; } }
		public bool ConnectionTypeMismatch { get { return SourcePort.ConnectionType != DestinationPort.ConnectionType; } }
		public string TypeMismatchNote { get; set; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sourcePort"></param>
		/// <param name="destinationPort"></param>
		public TieLine(RoutingOutputPort sourcePort, RoutingInputPort destinationPort)
		{
			if (sourcePort == null || destinationPort == null)
				throw new ArgumentNullException("source or destination port");
			SourcePort = sourcePort;
			DestinationPort = destinationPort;
		}

		/// <summary>
		/// Creates a tie line with an overriding Type.  See help for OverrideType property for info
		/// </summary>
		/// <param name="overrideType">The signal type to limit the link to. Overrides DestinationPort.Type</param>
		public TieLine(RoutingOutputPort sourcePort, RoutingInputPort destinationPort, eRoutingSignalType overrideType) :
			this(sourcePort, destinationPort)
		{
			OverrideType = overrideType;
		}

		public static TieLine TieLineFromStrings(string sourceKey, string sourcePortKey, string destinationKey, string destinationPortKey)
		{
			var sourceDev = DeviceManager.GetDeviceForKey(sourceKey) as IRoutingOutputs;
			if (sourceDev == null)
			{
				Debug.Console(1, "WARNING: Cannot create tie line, routable source '{0}' not found", sourceKey);
				return null;
			}
			var destDev = DeviceManager.GetDeviceForKey(destinationKey) as IRoutingInputs;
			if (destDev == null)
			{
				Debug.Console(1, "WARNING: Cannot create tie line, routable destination '{0}' not found", destinationKey);
				return null;
			}
			var sourcePort = sourceDev.OutputPorts[sourcePortKey];
			if (sourcePort == null)
			{
				Debug.Console(1, "WARNING: Cannot create tie line. Source '{0}' does not contain port '{1}'", sourceKey, sourcePortKey);
				return null;
			}
			var destPort = destDev.InputPorts[destinationPortKey];
			if (destPort == null)
			{
				Debug.Console(1, "WARNING: Cannot create tie line. Destination '{0}' does not contain port '{1}'", destinationKey, destinationPortKey);
				return null;
			}

			return new TieLine(sourcePort, destPort);
		}

		/// <summary>
		/// Will link up video status from supporting inputs to connected outputs
		/// </summary>
		public void Activate()
		{
			// Now does nothing
		}

		public void Deactivate()
		{
			// Now does nothing
		}

		public override string ToString()
		{
			return string.Format("Tie line: [{0}]{1} --> [{2}]{3}", SourcePort.ParentDevice.Key, SourcePort.Key,
				DestinationPort.ParentDevice.Key, DestinationPort.Key);
		}
	}


	//********************************************************************************

	public class TieLineCollection : List<TieLine>
	{
		public static TieLineCollection Default
		{
			get
			{
				if (_Default == null)
					_Default = new TieLineCollection();
				return _Default;
			}
		}
		static TieLineCollection _Default;
	}
}