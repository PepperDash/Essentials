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
using PepperDash.Essentials.Core.Routing;

namespace PepperDash.Essentials.Devices.Common
{
	public class GenericSource : EssentialsDevice, IUiDisplayInfo, IRoutingOutputs, IUsageTracking
	{

		public uint DisplayUiType { get { return DisplayUiConstants.TypeNoControls; } }

        public GenericSource(string key, string name)
			: base(key, name)
		{

            AnyOut = new RoutingOutputPort(RoutingPortNames.AnyOut, eRoutingSignalType.Audio | eRoutingSignalType.Video, 
				eRoutingPortConnectionType.Hdmi, null, this);
			OutputPorts = new RoutingPortCollection<RoutingOutputPort> { AnyOut };
		}

		#region IRoutingOutputs Members

		public RoutingOutputPort AnyOut { get; private set; }
		public RoutingPortCollection<RoutingOutputPort> OutputPorts { get; private set; }

		#endregion

        #region IUsageTracking Members

        public UsageTracking UsageTracker { get; set; }

        #endregion
	}

    public class GenericSourceFactory : EssentialsDeviceFactory<GenericSource>
    {
        public GenericSourceFactory()
        {
            TypeNames = new List<string>() { "genericsource" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new Generic Source Device");
            return new GenericSource(dc.Key, dc.Name);
        }
    }

}