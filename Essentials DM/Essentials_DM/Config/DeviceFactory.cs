using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharpPro;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.DM
{
	public class DeviceFactory
	{
		public static IKeyed GetDevice(DeviceConfig dc)
		{
			var key = dc.Key;
			var name = dc.Name;
			var type = dc.Type;
			var properties = dc.Properties;

			var typeName = dc.Type.ToLower();

			if (typeName == "dmmd8x8")
			{
				var props = JsonConvert.DeserializeObject
					<PepperDash.Essentials.DM.Config.DMChassisPropertiesConfig>(properties.ToString());
				return PepperDash.Essentials.DM.DmChassisController.
					GetDmChassisController(key, name, type, props);
			}

			// Hand off to DmTxHelper class
			else if (typeName.StartsWith("dmtx"))
			{
				var props = JsonConvert.DeserializeObject
					<PepperDash.Essentials.DM.Config.DmTxPropertiesConfig>(properties.ToString());
				return PepperDash.Essentials.DM.DmTxHelper.GetDmTxController(key, name, type, props);
			}

			// Hand off to DmRmcHelper class
			else if (typeName.StartsWith("dmrmc"))
			{
				var props = JsonConvert.DeserializeObject
					<PepperDash.Essentials.DM.Config.DmRmcPropertiesConfig>(properties.ToString());
				return PepperDash.Essentials.DM.DmRmcHelper.GetDmRmcController(key, name, type, props);
			}

			return null;
		}


	}

}