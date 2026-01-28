using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core.Config;
using Serilog.Events;

namespace PepperDash.Essentials.Core.Web
{
 /// <summary>
 /// Represents a EssentialsWebApiFactory
 /// </summary>
	public class EssentialsWebApiFactory : EssentialsDeviceFactory<EssentialsWebApi>
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public EssentialsWebApiFactory()
		{
			TypeNames = new List<string> { "EssentialsWebApi" };
		}

		/// <summary>
		/// BuildDevice method
		/// </summary>
		/// <inheritdoc />
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