using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core
{
	public class CrestronWebServerFactory : EssentialsDeviceFactory<CrestronWebServerBase>
	{
		public CrestronWebServerFactory()
		{
			TypeNames = new List<string> { "crestroncws", "cws" };
		}
		public override EssentialsDevice BuildDevice(DeviceConfig dc)
		{
			Debug.Console(1, "Factory Attempting to create new Crestron CWS Device");

			return new CrestronWebServerBase(dc.Key, dc.Name, "");
		}
	}
}