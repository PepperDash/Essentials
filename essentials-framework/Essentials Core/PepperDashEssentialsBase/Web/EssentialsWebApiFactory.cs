using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core.Web
{
	public class EssentialsWebApiFactory : EssentialsDeviceFactory<EssemtialsWebApi>
	{
		public EssentialsWebApiFactory()
		{
			TypeNames = new List<string> { "EssentialsWebApi" };
		}

		public override EssentialsDevice BuildDevice(DeviceConfig dc)
		{
			Debug.Console(1, "Factory Attempting to create new Essentials Web API Server");

			var props = dc.Properties.ToObject<EssentialsWebApiPropertiesConfig>();
			if (props != null) return new EssemtialsWebApi(dc.Key, dc.Name, props);

			Debug.Console(1, "Factory failed to create new Essentials Web API Server");
			return null;
		}
	}
}