using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using Serilog.Events;

namespace PepperDash.Essentials.Devices.Common
{
 /// <summary>
 /// Represents a GenericSource
 /// </summary>
	public class GenericSource : EssentialsDevice, IUiDisplayInfo, IRoutingSource, IUsageTracking
	{

  /// <summary>
  /// Gets or sets the DisplayUiType
  /// </summary>
		public uint DisplayUiType { get { return DisplayUiConstants.TypeNoControls; } }

        public GenericSource(string key, string name)
			: base(key, name)
		{

            AnyOut = new RoutingOutputPort(RoutingPortNames.AnyOut, eRoutingSignalType.Audio | eRoutingSignalType.Video, 
				eRoutingPortConnectionType.Hdmi, null, this);
			OutputPorts = new RoutingPortCollection<RoutingOutputPort> { AnyOut };
		}

		#region IRoutingOutputs Members

  /// <summary>
  /// Gets or sets the AnyOut
  /// </summary>
		public RoutingOutputPort AnyOut { get; private set; }
  /// <summary>
  /// Gets or sets the OutputPorts
  /// </summary>
		public RoutingPortCollection<RoutingOutputPort> OutputPorts { get; private set; }

		#endregion

        #region IUsageTracking Members

        /// <summary>
        /// Gets or sets the UsageTracker
        /// </summary>
        public UsageTracking UsageTracker { get; set; }

        #endregion
	}

    /// <summary>
    /// Represents a GenericSourceFactory
    /// </summary>
    public class GenericSourceFactory : EssentialsDeviceFactory<GenericSource>
    {
        public GenericSourceFactory()
        {
            TypeNames = new List<string>() { "genericsource" };
        }

        /// <summary>
        /// BuildDevice method
        /// </summary>
        /// <inheritdoc />
        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.LogMessage(LogEventLevel.Debug, "Factory Attempting to create new Generic Source Device");
            return new GenericSource(dc.Key, dc.Name);
        }
    }
}