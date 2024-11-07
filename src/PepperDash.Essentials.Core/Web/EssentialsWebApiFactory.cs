using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Devices;
using Serilog.Events;

namespace PepperDash.Essentials.Core.Web
{
	public class EssentialsWebApiFactory : EssentialsDeviceFactory<EssentialsWebApi>
	{
		public EssentialsWebApiFactory()
		{
			TypeNames = new List<string> { "EssentialsWebApi" };
		}

		public override EssentialsDevice BuildDevice(DeviceConfig dc)
		{
			Debug.LogMessage(LogEventLevel.Debug, "Factory Attempting to create new Essentials Web API Server");

			var props = dc.Properties.ToObject<EssentialsWebApiPropertiesConfig>();
			if (props != null) return new EssentialsWebApi(dc.Key, dc.Name, props);

			Debug.LogMessage(LogEventLevel.Debug, "Factory failed to create new Essentials Web API Server");
			return null;
		}
	}
}